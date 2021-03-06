﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RoundTripAddIn
{
    public class RepositoryHelper
    {
        static Logger logger = new Logger();

        static public void setLogger(Logger l)
        {
            logger = l;            
        }

        static public EA.Element queryClassifier(EA.Repository Repository, String classifierName)
        {
            string resultDoc = Repository.SQLQuery("select Object_ID from t_object where Name='" + classifierName + "' and Object_Type='Class'");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(resultDoc);
            List<string> result = new List<string>();
            foreach (XmlNode node in doc.GetElementsByTagName("Object_ID"))
            {
                logger.log("Found ClassifierId for:" + classifierName + " as " + node.InnerText);
                return Repository.GetElementByID(Int32.Parse(node.InnerText));
            }
            return null;
        }


        static void cacheDiagramElements(EA.Repository repository, EA.Collection collection, DiagramCache diagramCache)
        {
            //diagramCache.elementsList = new List<EA.Element>();
            Object o;
            EA.Element e;
            if (collection.Count == 0)
                return;

            int eId, pkgId;
            EA.Collection elements = null;

            List<int> els = new List<int>();
            for (short i = 0; i < collection.Count; i++)
            {
                o = collection.GetAt(i);
                eId = ((EA.DiagramObject)o).ElementID;
                if (diagramCache.elementIDHash.ContainsKey(eId))//Skip elements already cached
                    continue;
                els.Add(eId);                  
            }


            StringBuilder sb;
            {                
                sb = new StringBuilder();
                for (short i = 0; i < els.Count - 1; i++)
                {
                    eId = els[i];                                       
                    sb.Append(eId);
                    sb.Append(",");
                }
                eId = els[(short)(els.Count - 1)];                
                sb.Append(eId);
                String elementsString = sb.ToString();
                //logger.log("DiagramObjectIds"+sb.ToString());
                //logger.log("Getting elements:"+elementsString);

                elements = repository.GetElementSet(elementsString, 0);
            }
            sb = new StringBuilder();

            if (elements != null)
            {
                foreach (EA.Element el in elements)
                {
                    diagramCache.elementsList.Add(el);
                    if (!diagramCache.elementIDHash.ContainsKey(el.ElementID))
                        diagramCache.elementIDHash.Add(el.ElementID, el);
                    if (!diagramCache.elementGuidHash.ContainsKey(el.ElementGUID))
                        diagramCache.elementGuidHash.Add(el.ElementGUID, el);
                }
            }                    
        }

        static void populateCache(EA.Collection elements,DiagramCache diagramCache)
        {
           
        }

        static void cacheDiagramClassifiers(EA.Repository repository, DiagramCache diagramCache)
        {            
            Object o;
            if (diagramCache.elementsList.Count == 0)
                return;

            int classifierId;
            StringBuilder sb = new StringBuilder();
            for (short i = 0; i < diagramCache.elementsList.Count; i++)
            {
                o = diagramCache.elementsList[i];
                EA.Element el = (EA.Element)o;
                if (!diagramCache.elementIDHash.ContainsKey(el.ElementID))
                {
                    diagramCache.elementIDHash.Add(el.ElementID, el);
                }
                classifierId = el.ClassifierID;
                if (classifierId != 0)
                {
                    sb.Append(classifierId);
                    sb.Append(",");
                }                
            }
            logger.log("Getting classifiers"+ sb.ToString());
            
            String classifiersString = sb.ToString();
            if (classifiersString.Length == 0)
                return;
            classifiersString = classifiersString.Substring(0, classifiersString.Length - 1);
            EA.Collection classifierCollection = repository.GetElementSet(classifiersString, 0);

            logger.log("Classifiers Length" + classifierCollection.Count);

            int elementId;
            for (short i = 0; i < classifierCollection.Count; i++)
            {
                logger.log("Get classifier" + i);
                logger.log(classifierCollection.GetType().Name);
               
                o = classifierCollection.GetAt(i);

                EA.Element element = (EA.Element)o;
                elementId = element.ElementID;
                logger.log("A");
                if (!diagramCache.elementIDHash.ContainsKey(elementId))
                {
                    diagramCache.elementIDHash.Add(elementId, element);
                }
                logger.log("B");
                if (!diagramCache.elementGuidHash.ContainsKey(element.ElementGUID))
                {
                    diagramCache.elementGuidHash.Add(element.ElementGUID, element);
                }
            }                        
        }


        static void cacheDiagramPackages(EA.Repository repository, DiagramCache diagramCache)
        {
            Object o;
            if (diagramCache.elementsList.Count == 0)
                return;
            
            StringBuilder sb = new StringBuilder();
            for (short i = 0; i < diagramCache.elementsList.Count; i++)
            {
                o = diagramCache.elementsList[i];
                EA.Element el = (EA.Element)o;
                if (!diagramCache.packageIDHash.ContainsKey(el.PackageID))
                {
                    //logger.log("Get PKG" + el.PackageID);
                    EA.Package pkg = repository.GetPackageByID(el.PackageID);
                    diagramCache.packageIDHash.Add(el.PackageID, pkg);
                }
                
            }            
        }


        public static DiagramCache createDiagramCache(EA.Repository repository, EA.Diagram diagram, DiagramCache result)
        {                      
            cacheDiagramElements(repository, diagram.DiagramObjects,result);
            logger.log("cache elements");
            cacheDiagramClassifiers(repository, result);
            logger.log("cache classifier");
            cacheDiagramPackages(repository, result);
            logger.log(result.elementsList.Count + ":" + result.elementIDHash.Count);
            return result;
        }      
      }

    /*
     **  The purpose of this class is to cache elements from the diagram.
     *  The elements can be retrieved efficiently to the client  in one go and then accessed from the cache.
     *  Rather than querying each element one by one during the processing logic.
     */
    public class DiagramCache
    {
        //A list of the elements on the diagram
        public List<EA.Element> elementsList { get; set; }

        //A map of the elements, classifiers, packages making up the diagram
        public IDictionary<int, EA.Element> elementIDHash { get; set; }

        //A map of the  packages making up the diagram
        public IDictionary<int, EA.Package> packageIDHash { get; set; }

        //A map by guid of the elements, classifiers, packages making up the diagram
        public IDictionary<String, EA.Element> elementGuidHash { get; set; }

        public DiagramCache()
        {
            elementsList = new List<EA.Element>();
            elementIDHash = new Dictionary<int, EA.Element>();
            packageIDHash = new Dictionary<int, EA.Package>();
            elementGuidHash = new Dictionary<String, EA.Element>();
        }

        public void addElement(EA.Element element)
        {
            elementGuidHash[element.ElementGUID] = element;
            elementsList.Add(element);
        }

    }
}
