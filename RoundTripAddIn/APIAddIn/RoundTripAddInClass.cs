using System;
using System.Collections.Generic;
using System.Net;
using System.Windows.Forms;
using EA;
using SVGExport;

namespace RoundTripAddIn
{
    public class RoundTripAddInClass
    {
        public const string ADDIN_NAME = "RoundTrip";

        // define menu constants
        const string menuHeader = "-&RoundTrip";
        const string menuHeaderExperimental = "-&Experimental";

        const string menuWeb = "&Call Web";

        const string menuToggleLogging = "Toggle Logging";

        const string menuValidateDiagram = "&ValidateDiagram";

        const string menuExportDiagram = "&ExportDiagram";

        const string menuExportAll = "&ExportAll";
        const string menuExportPackage = "&ExportPackage";
     
        const string menuExportSchema = "&ExportSchemas";
        const string menuExportSample = "&ExportSamples";
        const string menuSyncSample = "&SyncDiagramSample";

        const string menuUpdateClassFromInstance = "&UpdateClassFromInstance";
        const string menuUpdateInstanceFromClass = "&UpdateInstanceFromClass";

        const string menuCreateSample = "&GenerateSample";

        const string menuExportPopulation = "&Export Population";
        const string menuExportHierarchy = "&Export Hierarchy";
        const string menuExportMapping = "&Export Mapping";
        const string menuSyncMapping = "&Sync Mapping";
        const string menuSyncPopulation = "&Sync Population";
        const string menuSyncHierarchy = "&Sync Hierarchy";
        const string menuExportConstraint = "&Export Constraints";
        const string menuSyncConstraint = "&Sync Constraints";

        const string menuSqlQuery = "&SqlQuery";

        static Logger logger = new Logger();

        static FileManager fileManager = new FileManager(RoundTripAddInClass.logger);

        public static string EA_TYPE_BOOLEAN = "boolean";
        public static string EA_TYPE_INT = "int";
        public static string EA_TYPE_DECIMAL = "decimal";
        public static string EA_TYPE_FLOAT = "float";
        public static string EA_TYPE_NUMBER = "number";
        public static string EA_TYPE_DATE = "date";
        public static string EA_TYPE_DATETIME = "datetime";
        public static string EA_TYPE_STRING = "String";
        public static string EA_TYPE_CURRENCY = "currency";
        public static string EA_TYPE_ATTRIBUTE = "Attribute";
        public static
        
        string EA_TYPE_ASSOCIATION = "Association";

        public static string EA_TYPE_CLASS = "Class";
        public static string EA_TYPE_COMPONENT = "Component";
        public static string EA_TYPE_OBJECT = "Object";
        public static string EA_TYPE_ENUMERATION = "Enumeration";
        public static string EA_TYPE_PACKAGE = "Package";

        public static string EA_STEREOTYPE_NONE = "";
        public static string EA_STEREOTYPE_SCHEMADIAGRAM = "SchemaDiagram";
        public static string EA_STEREOTYPE_SAMPLEDIAGRAM = "SampleDiagram";
        public static string EA_STEREOTYPE_POPULATIONDIAGRAM = "PopulationDiagram";
        public static string EA_STEREOTYPE_MAPPINGDIAGRAM = "MappingDiagram";
        public static string EA_STEREOTYPE_HIERARCHYDIAGRAM = "HierarchyDiagram";
        public static string EA_STEREOTYPE_CONSTRAINTDIAGRAM = "ConstraintDiagram";

        public static string EA_STEREOTYPE_SAMPLE = "Sample";
        public static string EA_STEREOTYPE_REQUEST = "Request";
        public static string EA_STEREOTYPE_DATAITEM = "DataItem";
        public static string EA_STEREOTYPE_POPULATION = "Population";
        public static string EA_STEREOTYPE_HIERARCHY = "Hierarchy";
        public static string EA_STEREOTYPE_MAPPING = "Mapping";
        public static string EA_STEREOTYPE_CONSTRAINT = "Constraint";

        public static string EA_STEREOTYPE_EMBEDDED = "Embedded";

        public static string EA_TAGGEDVALUE_PATTERN = "Pattern";
        public static string EA_TAGGEDVALUE_DEFAULT = "Default";
        
        public static string API_PACKAGE_SCHEMAS = "Schemas";
        public static string API_PACKAGE_SAMPLES = "Samples";

        public static string METAMODEL_API = "API";
        public static string METAMODEL_CONTENTTYPE = "ContentType";
        public static string METAMODEL_RESPONSE = "Response";
        public static string METAMODEL_RESOURCE = "Resource";
        public static string METAMODEL_RESOURCETYPE = "ResourceType";
        public static string METAMODEL_ITEMGET = "ItemGet";
        public static string METAMODEL_SECURITYSCHEME = "SecurityScheme";
        public static string METAMODEL_QUERY_PARAMETER = "QueryParameter";
        public static string METAMODEL_COMMUNITY = "Community";
        public static string METAMODEL_SAMPLE = "Sample";
        public static string METAMODEL_DATAITEM = "DataItem";
        public static string METAMODEL_TRAIT = "Trait";
        public static string METAMODEL_RELEASEPIPELINE = "ReleasePipeline";
        public static string METAMODEL_ENVIRONMENT = "Environment";
        public static string METAMODEL_PERMISSION = "Permission";

        public static string METAMODEL_PLACEHOLDER = "PlaceHolder";

        public static string RESOURCETYPE_ITEMGET = "item-get";
        public static string RESOURCETYPE_ITEMPOST = "item-post";
        public static string RESOURCETYPE_ITEMPOST_SYNC = "item-post-sync";
        public static string RESOURCETYPE_ITEMPOST_ONEWAY = "item-post-oneway";
        public static string RESOURCETYPE_COLLECTIONGETPOST = "collection-get-post";

        public static string METAMODEL_SCHEMA = "Schema";
        public static string METAMODEL_METHOD = "Method";
        public static string METAMODEL_TYPE_FOR_RESOURCE = "TypeForResource";
        public static string METAMODEL_SERVER = "Server";

        public static string CARDINALITY_0_TO_MANY = "0..*";
        public static string CARDINALITY_1_TO_MANY = "1..*";
        public static string CARDINALITY_0_TO_ONE = "0..1";
        public static string CARDINALITY_ONE = "1";

        public static string DIRECTION_SOURCE_TARGET = "Source -> Destination";

        public static string MARKDOWN_PARAGRAPH_BREAK = "\n";

        public static double RAML_0_8 = 0.8;
        public static double RAML_1_0 = 1.0;
        public static double[] RAML_VERSIONS = new double[2] { RAML_0_8, RAML_1_0 } ;

        public static string EXPORT_PACKAGE = "export";

        public static string PROJECT = "project";
        public static string FILENAME = "filename";
        public static string PREFIX = "prefix";
        public static string INCLUDE_INTERTYPE = "intertype";

        public static string POPULATION_PATH = "population";
        public static string POPULATION_PROPERTY_GUID = "id";
        public static string POPULATION_PROPERTY_NAME = "name";
        public static string POPULATION_PROPERTY_NOTES = "notes";
        public static string POPULATION_PROPERTY_TYPE = "type";
        public static string POPULATION_PROPERTY_PACKAGE = "package";

        public static string HIERARCHY_LEVEL = "level";
        public static string HIERARCHY_PATH = "hierarchy";
        public static string HIERARCHY_PROPERTY_TYPE = "type";
        public static string HIERARCHY_PROPERTY_ID = "id";        
        public static string HIERARCHY_PROPERTY_PARENT = "parent";
        public static string HIERARCHY_PROPERTY_NAME = "name";
        public static string HIERARCHY_PROPERTY_DESCRIPTION = "description";
        public static string HIERARCHY_PROPERTY_LEVEL = "min";

        public static string MAPPING_PATH = "map";
        public static string MAPPING_PROPERTY_ID = "id";
        public static string MAPPING_PROPERTY_NAME = "name";
        public static string MAPPING_PROPERTY_NOTES = "notes";
        public static string MAPPING_PROPERTY_SOURCE = "source";
        public static string MAPPING_PROPERTY_SOURCE_NAME = "sourcename";
        public static string MAPPING_PROPERTY_SOURCE_CLASS = "sourceclass";
        public static string MAPPING_PROPERTY_TARGET = "target";
        public static string MAPPING_PROPERTY_TARGET_NAME = "targetname";
        public static string MAPPING_PROPERTY_TARGET_CLASS = "targetclass";
        public static string MAPPING_PROPERTY_TARGET_VALUE = "value";
        public static string MAPPING_PROPERTY_TYPE = "type";
        public static string MAPPING_PROPERTY_STEREOTYPE = "stereotype";

        public static string CONSTRAINT_PATH = "constraint";
        public static string CONSTRAINT_PROPERTY_GUID = "id";
        public static string CONSTRAINT_PROPERTY_SOURCE = "source";
        public static string CONSTRAINT_PROPERTY_TARGET = "target";
        public static string CONSTRAINT_PROPERTY_NAME = "name";
        public static string CONSTRAINT_PROPERTY_CLASS = "class";
        public static string CONSTRAINT_PROPERTY_STEREOTYPE = "stereotype";
        public static string CONSTRAINT_PROPERTY_RELATED_GUID = "id";
        public static string CONSTRAINT_PROPERTY_RELATED_NAME = "targetname";
        public static string CONSTRAINT_PROPERTY_RELATED_CLASS = "targetclass";
        public static string CONSTRAINT_PROPERTY_CONNECTOR_GUID = "connectorid";
        public static string CONSTRAINT_PROPERTY_CONNECTOR_TYPE = "type";
        
        public static string PROPERTY_SELECTOR = "selector";

        ///
        /// Called Before EA starts to check Add-In Exists
        /// Nothing is done here.
        /// This operation needs to exists for the addin to work
        ///
        /// <param name="Repository" />the EA repository
        /// a string
        public String EA_Connect(EA.Repository Repository)
        {
            logger.setRepository(Repository);

            try
            {
                fileManager.setBasePath(Properties.Settings.Default.BasePath);
                fileManager.setDiagramPath(Properties.Settings.Default.DiagramPath);
            }
            catch (Exception)
            {
                logger.log("Did not find BasePath or DiagramPath in user settings");
            }

            MappingManager.setLogger(logger);
            MappingManager.setFileManager(fileManager);

            HierarchyManager.setLogger(logger);
            HierarchyManager.setFileManager(fileManager);

            PopulationManager.setLogger(logger);
            PopulationManager.setFileManager(fileManager);

            ConstraintManager.setLogger(logger);
            ConstraintManager.setFileManager(fileManager);

            DiagramManager.setLogger(logger);
            DiagramManager.setFileManager(fileManager);
            
            SchemaManager.setLogger(logger);
            SchemaManager.setFileManager(fileManager);
            SampleManager.setLogger(logger);
            SampleManager.setFileManager(fileManager);
            
            MetaDataManager.setLogger(logger);
            RepositoryHelper.setLogger(logger);
            ObjectManager.setLogger(logger);

            return "a string";
        }
 
        ///
        /// Called when user Clicks Add-Ins Menu item from within EA.
        /// Populates the Menu with our desired selections.
        /// Location can be "TreeView" "MainMenu" or "Diagram".
        ///
        /// <param name="Repository" />the repository
        /// <param name="Location" />the location of the menu
        /// <param name="MenuName" />the name of the menu
        ///
        public object EA_GetMenuItems(EA.Repository Repository, string Location, string MenuName)
        {
            logger.log("location:" + Location);
            logger.log("MenuName:" + MenuName);
            logger.log("ContextItemType:" + Repository.GetContextItemType());

            EA.Diagram diagram = null;
            if( Repository.GetContextItemType() == ObjectType.otDiagram )
                diagram = Repository.GetContextObject();
            else if (Repository.GetContextItemType() == ObjectType.otElement)
                diagram = Repository.GetCurrentDiagram();

            switch (MenuName)
            {
                // defines the top level menu option
                case "":                        
                    return menuHeader;
                                                                
                case menuHeader:
                    string[] subMenusOther = { menuExportPackage, menuExportAll, menuExportDiagram, menuToggleLogging};
                    //string[] subMenusAPI = { menuExportPackage, menuExportAll, menuExportDiagram, menuExportAPI, menuExportAPIRAML1, menuValidateDiagram, menuUpdateClassFromInstance, menuUpdateInstanceFromClass, menuToggleLogging };                    
                    string[] subMenusSchema = {  menuExportPackage, menuExportAll, menuExportDiagram, menuValidateDiagram, menuExportSchema, menuCreateSample, menuUpdateClassFromInstance, menuUpdateInstanceFromClass, menuToggleLogging };
                    string[] subMenusSample = { menuExportPackage, menuExportAll, menuExportDiagram, menuExportSample, menuValidateDiagram, menuSyncSample, menuUpdateClassFromInstance, menuUpdateInstanceFromClass, menuToggleLogging };
                    string[] subMenusPopulation = { menuExportPopulation, menuSyncPopulation, menuExportDiagram , menuToggleLogging};
                    string[] subMenusHierarchy = { menuExportHierarchy, menuSyncHierarchy, menuExportDiagram, menuToggleLogging };
                    string[] subMenusMapping = { menuExportMapping, menuSyncMapping, menuExportDiagram, menuToggleLogging };
                    string[] subMenusConstraint = { menuExportConstraint, menuSyncConstraint, menuExportDiagram, menuToggleLogging };
                    //string[] subMenusCanonical = { menuExportAll, menuExportDiagram, menuExportCanonical, menuCreateSample, menuUpdateClassFromInstance, menuUpdateInstanceFromClass, menuToggleLogging };

                    if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM))
                    {
                        logger.log("Schema Menus");
                        return subMenusSchema;
                    }
                    else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM))
                    {
                        logger.log("Sample Menus");
                        return subMenusSample;
                    }
                    else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM))
                    {
                        logger.log("Population Menus");
                        return subMenusPopulation;
                    }
                    else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_HIERARCHYDIAGRAM))
                    {
                        logger.log("Population Menus");
                        return subMenusHierarchy;
                    }
                    else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_MAPPINGDIAGRAM))
                    {
                        logger.log("Mapping Menus");
                        return subMenusMapping;
                    }
                    else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINTDIAGRAM))
                    {
                        logger.log("Constraint Menus");
                        return subMenusConstraint;
                    }
                    //else if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_CANONICALDIAGRAM))
                    //{
                    //    logger.log("Canonical Menus");
                    //    return subMenusCanonical;
                    //}

                    return subMenusOther;

                case menuHeaderExperimental:
                    string[] subMenus2 = { menuSqlQuery, menuWeb, };
                    //EA.Element apiEl = diagramAPI(Repository);
                    //if (apiEl == null)
                    //{
                    //    return new string[] { menuGenerate, menuGenerateSamples, menuGenerateAPI, menuValidateDiagram, };
                    //}                        
                    return subMenus2;
            }
 
            return "";
        }
 
        ///
        /// returns true if a project is currently opened
        ///
        /// <param name="Repository" />the repository
        /// true if a project is opened in EA
        bool IsProjectOpen(EA.Repository Repository)
        {
            try
            {
                EA.Collection c = Repository.Models;

                return true;
            }
            catch
            {
                return false;
            }
        }
 
        ///
        /// Called once Menu has been opened to see what menu items should active.
        ///
        /// <param name="Repository" />the repository
        /// <param name="Location" />the location of the menu
        /// <param name="MenuName" />the name of the menu
        /// <param name="ItemName" />the name of the menu item
        /// <param name="IsEnabled" />boolean indicating whethe the menu item is enabled
        /// <param name="IsChecked" />boolean indicating whether the menu is checked
        public void EA_GetMenuState(EA.Repository Repository, string Location, string MenuName, string ItemName, ref bool IsEnabled, ref bool IsChecked)
        {

            logger.log("Get Menu State:" + MenuName+":"+ItemName);

            if (IsProjectOpen(Repository))           
            {

                EA.Diagram diagram = null;
                if (Repository.GetContextItemType() == ObjectType.otDiagram)
                    diagram = Repository.GetContextObject(); 
                

                switch (ItemName)
                {
                    case menuWeb:
                        IsEnabled = true;
                        break;
                
                    
                    case menuValidateDiagram:
                        IsEnabled = true;
                        break;
                    // there shouldn't be any other, but just in case disable it.

                 
                    case menuExportSchema:
                    case menuCreateSample:
                        IsEnabled = false;
                        if (diagram != null &&  diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM))
                            IsEnabled = true;
                        break;


                    case menuExportSample:
                    case menuSyncSample:
                        IsEnabled = false;
                        if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM))
                            IsEnabled = true;
                        break;

                    case menuSyncPopulation:
                    case menuExportPopulation:
                        IsEnabled = false;
                        if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM))
                            IsEnabled = true;
                        break;

                   
                   
                    case menuExportMapping:
                    case menuSyncMapping:
                        IsEnabled = false;
                        if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_MAPPINGDIAGRAM))
                            IsEnabled = true;
                        break;

                    case menuExportHierarchy:
                    case menuSyncHierarchy:
                        IsEnabled = false;
                        if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_HIERARCHYDIAGRAM))
                            IsEnabled = true;
                        break;
                   
                    case menuSyncConstraint:
                    case menuExportConstraint:
                        IsEnabled = false;
                        if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_CONSTRAINTDIAGRAM))
                            IsEnabled = true;
                        break;

                    case menuExportAll:
                        IsEnabled = false;
                        if (diagram != null)           
                            IsEnabled = true;
                        break;

                     //case menuExportCanonical:
                     //   IsEnabled = false;
                     //   if (diagram != null && diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_CANONICALDIAGRAM))
                     //       IsEnabled = true;
                     //   break;

                    default:
                        IsEnabled = true;
                        break;
                }
            }
            else
            {
                // If no open project, disable all menu options
                IsEnabled = false;
            }
        }
 
        ///
        /// Called when user makes a selection in the menu.
        /// This is your main exit point to the rest of your Add-in
        ///
        /// <param name="Repository" />the repository
        /// <param name="Location" />the location of the menu
        /// <param name="MenuName" />the name of the menu
        /// <param name="ItemName" />the name of the selected menu item
        public void EA_MenuClick(EA.Repository Repository, string Location, string MenuName, string ItemName)
        {
            logger.enable(Repository);

            DiagramCache diagramCache = new DiagramCache();

            EA.Diagram diagram = null;
            if (Repository.GetContextItemType() == ObjectType.otDiagram)
                diagram = Repository.GetContextObject(); 
                                   
            switch (ItemName)
            {
                case menuExportMapping:
                    MappingManager.exportMapping(Repository, diagram,diagramCache);                    
                    break;
                case menuSyncMapping:
                    MappingManager.syncMapping(Repository, diagram, diagramCache);
                    break;

                case menuExportHierarchy:
                    logger.log("Menu Export Hierarchy");
                    HierarchyManager.exportHierarchy(Repository, diagram, diagramCache);
                    
                    break;
                case menuExportPopulation:                    
                    PopulationManager.exportPopulation(Repository, diagram, diagramCache);
                    
                    break;
                case menuSyncPopulation:
                    PopulationManager.syncPopulation(Repository, diagram, diagramCache);
                    break;

                case menuSyncHierarchy:
                    HierarchyManager.syncHierarchy(Repository, diagram, diagramCache);
                    break;


                case menuExportConstraint:
                    ConstraintManager.exportConstraint(Repository, diagram, diagramCache);                    
                    break;
                case menuSyncConstraint:
                    ConstraintManager.syncConstraint(Repository, diagram, diagramCache);
                    break;

                case menuExportAll:                    
                    exportAll(Repository,diagramCache);
                    break;

                case menuExportPackage:
                    exportPackage(Repository, diagramCache);
                    break;

                case menuExportDiagram:
                    exportDiagram(Repository);
                    break;

                case menuExportSchema:
                    try
                    {
                        SchemaManager.exportSchema(Repository, diagram,diagramCache);
                        MetaDataManager.setAsSchemaDiagram(Repository, diagram);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }                    
                    break;

                //case menuExportCanonical:
                //    try
                //    {
                //        SchemaManager.exportCanonical(Repository, diagram);
                //        //MetaDataManager.setAsCanonicalDiagram(Repository, diagram);
                //    }
                //    catch (Exception ex)
                //    {
                //        MessageBox.Show(ex.Message);
                //    }
                //    break;
                
                case menuExportSample:                    
                    SampleManager.exportSample(Repository,diagram,diagramCache);
                    MetaDataManager.setAsSampleDiagram(Repository, diagram);
                    break;

                case menuSyncSample:
                    SampleManager.syncSample(Repository,diagram,diagramCache);
                    break;

                case menuCreateSample:
                    SchemaManager.generateSample(Repository,diagramCache);
                    break;

                case menuUpdateClassFromInstance:
                    SchemaManager.updateClassFromInstance(Repository);
                    break;

                case menuUpdateInstanceFromClass:
                    SchemaManager.operateOnSample(Repository, SchemaManager.updateSampleFromClass);                    
                    break;

                case menuValidateDiagram:
                    if (diagram != null)
                        if(diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SAMPLEDIAGRAM))
                            SampleManager.validateDiagram(Repository,diagram,diagramCache);
                        else if (diagram.Stereotype.Equals(RoundTripAddInClass.EA_STEREOTYPE_SCHEMADIAGRAM))
                        {
                            SchemaManager.validateDiagram(Repository, diagram,diagramCache);
                        }                            
                        
                    break;


                


                case menuToggleLogging:
                    logger.toggleLogging(Repository);                    
                    break;

                case menuWeb:
                    this.callWeb(Repository);
                    break;
                            
            }
        }

        private void exportPackage(EA.Repository Repository,DiagramCache diagramCache)
        {
            EA.Package pkg = Repository.GetTreeSelectedPackage();
            exportPackage(Repository, pkg,diagramCache);
        }

        private void exportPackage(EA.Repository Repository, EA.Package pkg,DiagramCache diagramCache)
        {
            exportRoundTripPackage(Repository, pkg,diagramCache);
            foreach (EA.Package p in pkg.Packages)
            {                
                exportPackage(Repository, p,diagramCache);//recurse
            }
        }

        private void exportAll(EA.Repository Repository,DiagramCache diagramCache)
        {
            EA.Diagram diagram = null;
            if (Repository.GetContextItemType() == ObjectType.otDiagram)
                diagram = Repository.GetContextObject(); 
                   
            EA.Package apiPackage = Repository.GetPackageByID(diagram.PackageID);

            exportRoundTripPackage(Repository, apiPackage,diagramCache);
        }

        private void exportRoundTripPackage(EA.Repository Repository, EA.Package package,DiagramCache diagramCache)
        {
            
            foreach (object obj in package.Diagrams)
            {
                EA.Diagram samplediagram = (EA.Diagram)obj;

               
                    //DiagramManager.exportDiagram(Repository, samplediagram, diagramCache);
                if (samplediagram.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_POPULATIONDIAGRAM)
                {
                    PopulationManager.exportPopulation(Repository, samplediagram, diagramCache);
                } else if (samplediagram.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_MAPPINGDIAGRAM)
                {
                    MappingManager.exportMapping(Repository, samplediagram, diagramCache);
                } else if (samplediagram.Stereotype == RoundTripAddInClass.EA_STEREOTYPE_HIERARCHYDIAGRAM)
                {
                    HierarchyManager.exportHierarchy(Repository, samplediagram, diagramCache);
                }
            }            
        }

        private void exportDiagram(EA.Repository Repository)
        {
            EA.Diagram diagram = null;
            if (Repository.GetContextItemType() == ObjectType.otDiagram)
                diagram = Repository.GetContextObject(); 

            DiagramManager.exportDiagram(Repository, diagram);                
        }


        private void exportAllGlobal(EA.Repository Repository)
        {
            DiagramCache diagramCache = new DiagramCache();
            {
                List<string> diagrams = DiagramManager.querySchemaDiagrams(Repository);
                foreach (string diagramId in diagrams)
                {
                    EA.Diagram diagram = Repository.GetDiagramByGuid(diagramId);
                    logger.log("Exporting Schema Diagram:" + diagram.Name);
                    SchemaManager.exportSchema(Repository, diagram,diagramCache);
                }
            }
            {
                List<string> diagrams = DiagramManager.querySampleDiagrams(Repository);
                foreach (string diagramId in diagrams)
                {
                    EA.Diagram diagram = Repository.GetDiagramByGuid(diagramId);

                    EA.Package samplePackage = Repository.GetPackageByID(diagram.PackageID);
                    EA.Package apiPackage = Repository.GetPackageByID(samplePackage.ParentID);

                    logger.log("Exporting Sample Diagram:" + diagram.Name + " from api package:" + apiPackage.Name);
                    SampleManager.exportSample(Repository, diagram,diagramCache);
                }
            }
        }

        //public List<string> queryAPIDiagrams2(EA.Repository Repository)
        //{
            
        //}

        //public List<string> queryAPIDiagrams2(EA.Repository Repository)
        //{
        //    EA.Collection diagrams = Repository.GetElementsByQuery(
        //        "StateMachine Diagrams", "");
        //    MessageBox.Show("here");
        //    List<string> result = new List<string>();
        //    foreach (object dia in diagrams)
        //    {
        //        EA.Diagram d = (EA.Diagram)dia;
        //        result.Add(d.DiagramGUID);                
        //    }
        //    return result;
        //}
    

        private void callWeb(EA.Repository Repository)
        {
            // Create a request for the URL. 
            WebRequest request = WebRequest.Create("http://xceptionale.com");
            // If required by the server, set the credentials.
            //request.Credentials = CredentialCache.DefaultCredentials;
            // Get the response.
            WebResponse response = request.GetResponse();
            // Display the status.
            string status = ((HttpWebResponse)response).StatusDescription;

            MessageBox.Show("Status:"+status);

            // Get the stream containing content returned by the server.
            //Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            //StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            //string responseFromServer = reader.ReadToEnd();
            // Display the content.
            //Console.WriteLine(responseFromServer);
            // Clean up the streams and the response.
            //reader.Close();
            response.Close();
        }

        private void callWeb2(EA.Repository Repository)
        {
            object o;
            EA.ObjectType type = Repository.GetContextItem(out o);
            MessageBox.Show("Type:" + type);

            EA.Element e = (EA.Element)o;
            MessageBox.Show("Name:" + e.Name);

        }

        ///
        /// EA calls this operation when it exists. Can be used to do some cleanup work.
        ///
        public void EA_Disconnect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
 
    }

   
   
}
