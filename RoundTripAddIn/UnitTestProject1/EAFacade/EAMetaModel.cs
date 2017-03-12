using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoundTripAddIn;

namespace UnitTestProject1.EAFacade
{
    public class EAMetaModel
    {
        public EAElement META_API;
        public EAElement META_RESOURCE;
        public EAElement META_RESOURCE_TYPE;
        public EAElement META_TYPE_FOR_RESOURCE;
        public EAElement META_SCHEMA;
        public EAElement META_METHOD;
        public EAElement META_TRAIT;
        //public EAElement META_DATAITEM;
        public EAElement META_RELEASEPIPELINE;
        public EAElement META_ENVIRONMENT;
        public EAElement META_PERMISSION;
        public EAElement META_QUERY_PARAMETER;
        public EAElement META_SERVER;


        public EAPackage apiPackage = null;
        public EAPackage soaPackage = null;
        public EAPackage schemaPackage = null;
        public EAPackage samplesPackage = null;
        public EAPackage samplePackage = null;
        public EAPackage populationPackage = null;

        public EADiagram apiDiagram = null;
        public EADiagram soaDiagram = null;
        public EADiagram schemaDiagram = null;
        public EADiagram sampleDiagram = null;
        public EADiagram populationDiagram = null;

        public EAMetaModel()
        {

            if(EARepository.Repository==null)
                EARepository.Repository = new EARepository();

            META_API = new EAElement();
            META_RESOURCE = new EAElement();
            META_RESOURCE_TYPE = new EAElement();
            META_TYPE_FOR_RESOURCE = new EAElement();
            META_SCHEMA = new EAElement();
            META_METHOD = new EAElement();
            META_TRAIT = new EAElement();
            //META_DATAITEM = new EAElement();
            META_RELEASEPIPELINE = new EAElement();
            META_ENVIRONMENT = new EAElement();
            META_PERMISSION = new EAElement();
            META_QUERY_PARAMETER = new EAElement();
            META_SERVER = new EAElement();

            META_API.Name = RoundTripAddInClass.METAMODEL_API;
            META_RESOURCE.Name = RoundTripAddInClass.METAMODEL_RESOURCE;
            META_RESOURCE_TYPE.Name = RoundTripAddInClass.METAMODEL_RESOURCETYPE;
            META_TYPE_FOR_RESOURCE.Name = RoundTripAddInClass.METAMODEL_TYPE_FOR_RESOURCE;
            META_SCHEMA.Name = RoundTripAddInClass.METAMODEL_SCHEMA;
            META_METHOD.Name = RoundTripAddInClass.METAMODEL_METHOD;
            META_TRAIT.Name = RoundTripAddInClass.METAMODEL_TRAIT;
            //META_DATAITEM.Name = RoundTripAddInClass.METAMODEL_DATAITEM;
            META_RELEASEPIPELINE.Name = RoundTripAddInClass.METAMODEL_RELEASEPIPELINE;
            META_ENVIRONMENT.Name = RoundTripAddInClass.METAMODEL_ENVIRONMENT;
            META_PERMISSION.Name = RoundTripAddInClass.METAMODEL_PERMISSION;
            META_QUERY_PARAMETER.Name = RoundTripAddInClass.METAMODEL_QUERY_PARAMETER;

            META_SERVER.Name = RoundTripAddInClass.METAMODEL_SERVER;

            apiPackage = new EAPackage("UnitTest");

            object o = apiPackage.Packages.AddNew(RoundTripAddInClass.API_PACKAGE_SCHEMAS, RoundTripAddInClass.EA_TYPE_PACKAGE);
            schemaPackage = (EAPackage)o;            
            schemaPackage.ParentID = apiPackage.PackageID;

            o = apiPackage.Packages.AddNew(RoundTripAddInClass.API_PACKAGE_SAMPLES, RoundTripAddInClass.EA_TYPE_PACKAGE);
            samplesPackage = (EAPackage)o;            
            samplesPackage.ParentID = apiPackage.PackageID;

            o = samplesPackage.Packages.AddNew("sample", RoundTripAddInClass.EA_TYPE_PACKAGE);
            samplePackage = (EAPackage)o;            
            samplePackage.ParentID = samplesPackage.PackageID;

            //o = apiPackage.Diagrams.AddNew("API Diagram","");
            //apiDiagram = (EADiagram)o;
            //apiDiagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_APIDIAGRAM;            

            o = schemaPackage.Diagrams.AddNew("Schema Diagram", "");
            schemaDiagram = (EADiagram)o;
            schemaDiagram.Name = "Unit Test Schema Diagram";
            schemaDiagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM;
            
            o = samplePackage.Diagrams.AddNew("Sample Diagram", "");
            sampleDiagram = (EADiagram)o;
            sampleDiagram.Name = "Unit Test Sample Diagram";
            sampleDiagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM;

            //soaPackage = new EAPackage("UnitTestSOA");
            //o = soaPackage.Diagrams.AddNew("SOA Diagram", "");
            //soaDiagram = (EADiagram)o;
            //soaDiagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_SOADIAGRAM;
            
            populationPackage = new EAPackage("PopulationPackage");
            //populationPackage.ParentID = samplesPackage.PackageID;

            o = populationPackage.Diagrams.AddNew("Population Diagram", "");
            populationDiagram = (EADiagram)o;
            populationDiagram.Name = "PopulationDiagram";
            populationDiagram.Stereotype = RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM;
        }

        public EAMetaModel setupAPIPackage()
        {
            EARepository.currentDiagram = apiDiagram;
            EARepository.currentPackage = apiPackage;
            return this;
        }
        public EAMetaModel setupSchemaPackage()
        {
            EARepository.currentDiagram = schemaDiagram;
            EARepository.currentPackage = schemaPackage;
            return this;
        }
        public EAMetaModel setupSamplePackage()
        {
            EARepository.currentDiagram = sampleDiagram;
            EARepository.currentPackage = samplePackage;
            return this;
        }
        public EAMetaModel setupSOAPackage()
        {
            EARepository.currentDiagram = soaDiagram;
            EARepository.currentPackage = soaPackage;
            return this;
        }
        public EAMetaModel setupPopulationPackage()
        {
            EARepository.currentDiagram = populationDiagram;
            EARepository.currentPackage = populationPackage;
            return this;
        }

    }
}
