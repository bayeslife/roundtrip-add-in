using System.Collections.Generic;
using System.IO;
using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using RoundTripAddIn;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

using UnitTestProject1.EAFacade;


namespace UnitTestProject1
{
    [TestClass]
    public class RoundTripTest
    {
        [TestMethod]
        public void TestConvert()
        {
            DateTime dt =Convert.ToDateTime("2016-01-01");


            DateTime dt2 = Convert.ToDateTime("2016-01-01T00:00:00Z");

            DateTime dt3 =  dt2.ToLocalTime();


            //Assert.AreEqual(dt,dt2);
            Assert.AreEqual(dt3, dt2);

    
        }

      

        [TestMethod]
        public void TestEAMock()
        {
            EAMetaModel meta = new EAMetaModel();

            EARepository rep = EARepository.Repository;

            EAElement metaAPI = new EAElement();
            metaAPI.Name = "API";

            EAElement api = new EAElement();
            api.Name = "api";
            api.Type = "Class";
            api.Stereotype = "stereotype";
            api.ClassifierID = metaAPI.ElementID;

            api.RunState = "runstate";

            EAElement resource = new EAElement();
            resource.Name = "/resource";

            EA.Collection connectors = api.Connectors;
            object con = connectors.AddNew("", RoundTripAddIn.RoundTripAddInClass.EA_TYPE_ASSOCIATION);

            EAConnector c = (EAConnector)con;

            c.ClientID = api.ElementID;
            c.SupplierID = resource.ElementID;

            c.SupplierEnd.Role = "SupplierRole";

            Assert.AreEqual(1, api.Connectors.Count);


            EAElement metaAPI2 = (EAElement)rep.GetElementByID(metaAPI.ElementID);
            Assert.AreEqual(metaAPI.ElementID, metaAPI2.ElementID);

            EAElement api2 = (EAElement)rep.GetElementByID(c.ClientID);
            EAElement resource2 = (EAElement)rep.GetElementByID(c.SupplierID);

            Assert.AreEqual(api.ElementID, api2.ElementID);
            Assert.AreEqual(resource.ElementID, resource2.ElementID);

            Assert.IsNotNull(c.SupplierEnd);
            Assert.IsNotNull(c.SupplierEnd.Role);
        }

    }
}