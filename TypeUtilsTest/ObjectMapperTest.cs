using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TypeUtils.Services;
using TypeUtils.Services.Impl;

namespace TypeUtilsTest
{
    public class Ford
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime TimeOfBirth { get; set; }
    }

    public class Arthur
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime TimeOfBirth { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }
    }

    [TestClass]
    public class ObjectMapperTest
    {
        public static IObjectMapper _mapper;
        public static List<Arthur> _performanceData;

        [ClassInitialize]
        public static void initClass(TestContext testContext)
        {
            _mapper = ObjectMapper.Current;

            var arthur = new Arthur()
            {
                Id = Guid.NewGuid(),
                FirstName = "Arthur",
                LastName = "Dent"
            };

            _performanceData = new List<Arthur>();
            for (int i = 0; i < 1000000; i++)
                _performanceData.Add(arthur);

            var mapping = new Mapping<Arthur, Ford>()
                .map("Id")
                .map("TimeOfBirth")
                .map("LastName", (source, target, value) =>
                {
                    target.Name = source.FirstName + " " + source.LastName;
                });

            _mapper.registerMapping(mapping);

            var mappingPerf = new Mapping<Arthur, Arthur>()
                .map("Id")
                .map("TimeOfBirth")
                .map("LastName")
                .map("FirstName")
                .map("Age")
                .map("Address");

            _mapper.registerMapping(mappingPerf);


            var mappingToDict = new Mapping<Arthur, Dictionary<string, object>>()
                .map("Id", (s, t, v) => t["Id"] = v)
                .map("TimeOfBirth", (s, t, v) => t["TimeOfBirth"] = v)
                .map("LastName", (s, t, v) => t["LastName"] = v)
                .map("FirstName", (s, t, v) => t["FirstName"] = v);

            _mapper.registerMapping(mappingToDict);
        }


        [TestMethod]
        public void SimpleMappingTest()
        {
            var source = new Arthur()
            {
                Id = Guid.NewGuid(),
                FirstName = "Arthur",
                LastName = "Dent",
                TimeOfBirth = DateTime.Now
            };

            var result = _mapper.map<Arthur, Ford>(new Arthur[] { source });

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(result.First().Id, source.Id);
            Assert.AreEqual(result.First().Name, "Arthur Dent");
        }

        [TestMethod]
        public void DictionaryMappingTest()
        {
            var source = new Arthur()
            {
                Id = Guid.NewGuid(),
                FirstName = "Arthur",
                LastName = "Dent",
                TimeOfBirth = DateTime.Now
            };

            var result = _mapper.map<Arthur, Dictionary<string, object>>(new Arthur[] { source });

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(result.First()["Id"], source.Id);
            Assert.AreEqual(result.First()["FirstName"], source.FirstName);
            Assert.AreEqual(result.First()["LastName"], source.LastName);
            Assert.AreEqual(result.First()["TimeOfBirth"], source.TimeOfBirth);
        }

        [TestMethod]
        public void Performance()
        {
            foreach (var o in _mapper.map<Arthur, Arthur>(_performanceData))
            {
            }
        }

    }
}
