/*---------------------------------------------------------------------------------------------
 *  Copyright (c) 2017 International Federation of Red Cross. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using Microsoft.AspNetCore.Mvc;
using System.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using System.Linq;
using Read.HealthRisks;
using Infrastructure.AspNet;
using Events;
using Events.External;
using Infrastructure.TextMessaging;
using Read.CaseReports;
using Read.DataCollectors;

namespace Web
{
    [Route("api/testdatagenerator")]
    public class TestDataGeneratorController : BaseController
    {
        private readonly IMongoDatabase _database;
        private readonly ITextMessageProcessors _textMessageProcessors;

        private string[] _phoneNumbers = new[] {
            "",         // missing
            "11111111", // DataCollector #1
            "22222222", // DataCollector #2
            "33333333", // DataCollector #3
            "00000000"  // Non existing data collector
        };

        

        public TestDataGeneratorController(IMongoDatabase database, ITextMessageProcessors textMessageProcessors)
        {
            _textMessageProcessors = textMessageProcessors;
            _database = database;
        }

        [HttpGet("all")]
        public void CreateAll()
        {
            CreateHealthRisks();
            CreateDataCollectors();
            CreateTextMessages();
        }

        [HttpGet("healthrisks")]
        public void CreateHealthRisks()
        {
            var _collection = _database.GetCollection<HealthRisk>("HealthRisk");
            _collection.DeleteMany(v => true);

            var healthRisks = JsonConvert.DeserializeObject<HealthRiskCreated[]>(System.IO.File.ReadAllText("./TestData/HealthRisks.json"));
            foreach (var healthRisk in healthRisks)
                Apply(healthRisk.Id, healthRisk);
        }

        [HttpGet("datacollectors")]
        public void CreateDataCollectors()
        {
            var _collection = _database.GetCollection<DataCollector>("DataCollector");
            _collection.DeleteMany(v => true);

            var dataCollectors = JsonConvert.DeserializeObject<DataCollectorAdded[]>(System.IO.File.ReadAllText("./TestData/DataCollectors.json"));

            int i = 0;
            foreach (var dataCollector in dataCollectors)
            {
                Apply(dataCollector.Id, dataCollector);
                Apply(Guid.NewGuid(), new PhoneNumberAdded
                {
                    DataCollectorId = dataCollector.Id,
                    PhoneNumber = _phoneNumbers[1 + (i++ % 3)] // Only using the middle 3 phone numbers
                });
            }
        }


        [HttpGet("producejsonfortextmessages")]
        public void ProduceJsonForTextMessages()
        {
            var events = new List<TextMessage>();
            var randomizer = new Random();
            var keywords = new[] { "" };
            var healthRisks = _database.GetCollection<HealthRisk>("HealthRisk").Find(Builders<HealthRisk>.Filter.Empty).ToList();
            var healthRiskIds = healthRisks.Take(5).Select(v => v.ReadableId).ToArray();
            var numbers = _phoneNumbers;

            for (int i = 0; i < 100; i++)
            {
                var message = randomizer.NextDouble() < 0.9 ? CreateValidMessage(healthRiskIds) : CreateInvalidMessage();

                var textMessage = new TextMessage()
                {
                    Id = Guid.NewGuid(),
                    Keyword = keywords[randomizer.Next(keywords.Length)],
                    OriginNumber = numbers[randomizer.Next(numbers.Length)],
                    Sent = DateTimeOffset.Now.AddSeconds(-randomizer.NextDouble() * 60 * 60 * 24 * 7 * 26), // last 26 weeks
                    ReceivedAtGatewayNumber = "0123456789",
                    Message = message
                };

                // Create location for half the messages
                if (randomizer.NextDouble() > 0.5)
                {
                    textMessage.Latitude = -80d + randomizer.NextDouble() * 80d;    // Latitude between -80 and 80 degrees
                    textMessage.Longitude = randomizer.NextDouble() * 360d;         // Longitude between 0 and 360 degrees
                }

                events.Add(textMessage);
            }

            System.IO.File.WriteAllText("./TestData/TextMessagesReceived.json", JsonConvert.SerializeObject(events, Formatting.Indented));
        }

        private string CreateInvalidMessage()
        {
            Random randomizer = new Random();
            if (randomizer.NextDouble() < 0.5)
                return "1#2#1#1#2#1";
            else
                return "Hello! My report is #1#0#0#2#3#0#2#3#0#2#3#0#2#3#0";
        }

        private string CreateValidMessage(int[] healthRiskIds)
        {
            Random randomizer = new Random();

            if (randomizer.NextDouble() > 0.7)
            {
                // Aggregate event: health risk # males under 5 # males over 5 # female under 5 # female over 5
                var incidents = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 2, 2, 3 };
                return $"{healthRiskIds[randomizer.Next(healthRiskIds.Length)]}#{incidents[randomizer.Next(incidents.Length)]}#{incidents[randomizer.Next(incidents.Length)]}#{incidents[randomizer.Next(incidents.Length)]}#{incidents[randomizer.Next(incidents.Length)]}";
            }
            else
            {
                // Single event: healt risk # sex # age
                return $"{healthRiskIds[randomizer.Next(healthRiskIds.Length)]}#{randomizer.Next(2) + 1}#{randomizer.Next(70) + 1}";
            }
        }


        [HttpGet("textmessages")]
        public void CreateTextMessages()
        {
            var _caseReportsCollection = _database.GetCollection<CaseReport>("CaseReport");
            _caseReportsCollection.DeleteMany(v => true);

            var textMessagesEvents = JsonConvert.DeserializeObject<TextMessage[]>(System.IO.File.ReadAllText("./TestData/TextMessages.json"));
            foreach (var message in textMessagesEvents)
            {
                try
                {
                    _textMessageProcessors.Process(message);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                }
            }

        }
    }
}
