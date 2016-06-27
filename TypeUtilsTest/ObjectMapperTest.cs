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
    }

    [TestClass]
    public class ObjectMapperTest
    {
        public static IObjectMapper _mapper;

        [ClassInitialize]
        public static void initClass(TestContext testContext)
        {
            var mapping = new Mapping<Arthur, Ford>()
                .map("Id")
                .map("TimeOfBirth")
                .map("LastName", (source, target, value) =>
                {
                    target.Name = source.FirstName + " " + source.LastName;
                });

            _mapper = ObjectMapper.Current;

            _mapper.registerMapping(mapping);
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
        public void Performance()
        {
            var source = new Arthur()
            {
                Id = Guid.NewGuid(),
                FirstName = "Arthur",
                LastName = "Dent"
            };

            var lst = new List<Arthur>();
            for (int i = 0; i < 1000000; i++)
                lst.Add(source);

            _mapper.mapParallel<Arthur, Ford>(lst);
        }
        
    }
}
