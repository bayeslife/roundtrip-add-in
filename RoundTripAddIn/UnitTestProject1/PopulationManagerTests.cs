using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoundTripAddIn;
using UnitTestProject1.EAModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using UnitTestProject1.EAFacade;

namespace UnitTestProject1
{
    [TestClass]
    public class PopulationManagerTests
    {
        [TestMethod]
        public void TestExportPopulation()
        {
            EAMetaModel meta = new EAMetaModel().setupPopulationPackage();
            
            EAFactory rootClass = EAModel.createPopulation(meta);
            meta.setupPopulationPackage();
            EA.Package package = EARepository.currentPackage;

       
            Assert.AreEqual(1, package.Diagrams.Count);
           
            object o = package.Diagrams.GetAt(0);
            EA.Diagram diagram = (EA.Diagram)o;

            Assert.AreEqual(2, package.Elements.Count);

            //Test
            DiagramCache diagramCache = RepositoryHelper.createDiagramCache(EARepository.Repository, diagram);            
            JArray jobject = (JArray)PopulationManager.sampleToJObject(EARepository.Repository, diagram,diagramCache)["json"];

            Assert.AreEqual(1, jobject.Count);
        }
    }
}
