/*---------------------------------------------------------------------------------------------
 *  Copyright (c) 2017 International Federation of Red Cross. All rights reserved.
 *  Licensed under the MIT License. See LICENSE in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using Infrastructure.Application;
using Infrastructure.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Read;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Web
{
    [Route("api/healthRisk")]
    public class HealthRiskController : Controller
    {
        public static readonly Feature Feature = "HealthRisk";

        private readonly IEventEmitter _eventEmitter;

        private readonly ILogger<HealthRiskController> _logger;

        public HealthRiskController(
            IEventEmitter eventEmitter,
            ILogger<HealthRiskController> logger)
        {
            _eventEmitter = eventEmitter;
            _logger = logger;
        }

        [HttpGet]
        public string[] Get()
        {
            ////for now we are making this list in code in future we want to put it is the database so
            ////one can edit or add new healthrisk
            return new string[] 
               {"Health Risk",
                "Acute haemorrhagic fever syndrome",
                "Acute viral hepatitis",
                "Adverse events following immunization (AEFI)",
                "Anthrax",
                "Buruli ulcer (Mycobacterium ulcerans disease)",
                "Chikungunya ",
                "Cholera ",
                "Dengue Fever",
                "Dengue Haemorrhagic Fever",
                "Dengue Shock Syndrome",
                "Diabetes",
                "Diarrhoea with blood (dysentery/Shigella) ",
                "Dracunculiasis",
                "Ebola or Marburg viral hemorrhagic Fevers",
                "Foodborne Illnesses",
                "Human influenza (caused by a new subtype) ",
                "Pandemic (H1N1) 2009 virus infection",
                "Hypertension",
                "Influenza-like Illness (ILI)",
                "Crimean-Congo Haemorrhagic Fever (CCHF)",
                "Lassa Fever",
                "Leprosy",
                "Lymphatic Filariasis",
                "Malaria (uncomplicated)",
                "Malaria (severe)",
                "Malnutrition (newborns)",
                "Malnutrition (children)",
                "Malnutrition (pregnant women)",
                "Maternal Deaths",
                "Measles",
                "Meningococcal Meningitis",
                "Neonatal tetanus",
                "New AIDS Cases",
                "Noma",
                "Onchocerciasis",
                "Plague",
                "Poliomyelitis (Acute flaccid paralysis)",
                "Rabies",
                "Rift Valley Fever (RVF)",
                "Severe Acute Respiratory Infections (SARIs) (persons ≥ 5 years old)",
                "Severe Acute Respiratory Syndrome (SARS)",
                "Pneumonia (Children under 5)",
                "Severe Pneumonia (Children under 5)",
                "Genital ulcer syndrome (non-vesicular)",
                "Urethral discharge syndrome",
                "Smallpox (Variola)",
                "Trachoma",
                "Trypanosomiasis",
                "Tuberculosis",
                "Typhoid Fever",
                "West Nile Fever",
                "Yellow fever"};
        }
    }
}