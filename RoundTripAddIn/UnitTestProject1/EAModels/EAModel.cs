using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTestProject1.EAFacade;
using RoundTripAddIn;
using EA;


namespace UnitTestProject1.EAModels
{
    class EAModel
    {

        static public EAFactory createModel1(EAMetaModel meta)
        {
            EAFactory factory = new EAFactory();

            meta.setupSchemaPackage();

            meta.setupSamplePackage();
            EAFactory sampleRoot = factory.setupClient("ObjectWithListAttribute", RoundTripAddInClass.EA_TYPE_OBJECT, RoundTripAddInClass.EA_STEREOTYPE_REQUEST, 0, null);            

            return sampleRoot;
        }

        static public EAFactory createPopulation(EAMetaModel meta)
        {
            EAFactory factory = new EAFactory();
            
            meta.setupPopulationPackage();

            EA.Package package = EARepository.currentPackage;
            EA.Diagram diagram = EARepository.currentDiagram;

            EAFactory populationIndicator = factory.setupClient("populationIndicator", RoundTripAddInClass.EA_TYPE_OBJECT, RoundTripAddInClass.EA_STEREOTYPE_POPULATION, meta.META_SERVER.ElementID, null);


            EAFactory populationMember = factory.setupClient("populationMember", RoundTripAddInClass.EA_TYPE_OBJECT, null, meta.META_SERVER.ElementID, new string[] { "Name", "Name1", "version", "1" });

            //Object o = package.Diagrams.AddNew("Population-Sample", "Object");
            //EA.Diagram newdia = (EA.Diagram)o;
            EA.Diagram newdia = diagram;

            EARepository.currentPackage.Diagrams.Refresh();
            newdia.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM;
            newdia.Update();
            EARepository.currentPackage.Update();

            //logger.log("Added diagram:" + newdia.DiagramID);
                                 

            //Object o2 = diagram.DiagramObjects.AddNew("Indicator", "");
            //EA.DiagramObject diaObj2 = (EA.DiagramObject)o2;

            //diagram.Update();        
            //diaObj2.ElementID = populationIndicator.clientElement.ElementID;
            //diaObj.Update();
            
            //Object o3 = diagram.DiagramObjects.AddNew("Item", "");
            //EA.DiagramObject diaObj3 = (EA.DiagramObject)o3;
            //diagram.Update();        
            //diaObj3.ElementID = populationMember.clientElement.ElementID;

            return factory;
        }
    }
}
