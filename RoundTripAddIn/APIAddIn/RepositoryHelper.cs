using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RoundTripAddIn
{
    class RepositoryHelper
    {
        static Logger logger = new Logger();

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
    }
}
