﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeUtils.Services;

namespace TypeUtilsTest
{
    public class Marvin
    {
        public Guid A { get; set; }
        public int B { get; set; }
        public DateTime C { get; set; }
        public float D { get; set; } // Some missing attribute in Triallian
        public bool E { get; set; } 
    }

    public class Trillian
    {
        public string A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public string E { get; set; }
    }

    [TestClass]
    public class MapperTest
    {
        [TestMethod]
        public void TestSimpleObjectMapping()
        {
            var mapper = new ObjectMapper();

            var source = new Trillian()
            {
                A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                B = 1234.567,
                C = 42735.9998842593,
                E = "true"
            };

            var target = new Marvin();

            mapper.map(source, target);

            Assert.AreEqual(Guid.Parse("BE5B1F80-B878-44B4-8B61-DEADBEEF0000"), target.A);
            Assert.AreEqual(1235, target.B);
            Assert.AreEqual(new DateTime(2016, 12, 31, 23, 59, 50), target.C);
            Assert.AreEqual(true, target.E);
        }

        [TestMethod]
        public void MapAllTest()
        {
            var mapper = new ObjectMapper();

            var sources = new List<Trillian>()
            {
                new Trillian()
                {
                    A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                    B = 1234.567,
                    C = 42735.9998842593,
                    E = "true"
                },
                new Trillian()
                {
                    A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                    B = 654321,
                    C = 42735.9998842593,
                    E = "true"
                },
                new Trillian()
                {
                    A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                    B = 1234.567,
                    C = 42735.9998842593,
                    E = "true"
                }
            };

            var res = mapper.mapAll<Trillian, Marvin>(sources);

            Assert.AreEqual(Guid.Parse("BE5B1F80-B878-44B4-8B61-DEADBEEF0000"), res[1].A);
            Assert.AreEqual(654321, res[1].B);
            Assert.AreEqual(new DateTime(2016, 12, 31, 23, 59, 50), res[1].C);
            Assert.AreEqual(true, res[1].E);
        }


        [TestMethod]
        public void Performance()
        {
            var mapper = new ObjectMapper();

            var sources = new List<Trillian>();
            for (int i = 0; i < 1000000; i++)
            {
                sources.Add(new Trillian()
                {
                    A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                    B = 1234.567,
                    C = 42735.9998842593,
                    E = "true"
                });
            }
            mapper.mapAll<Trillian, Marvin>(sources);
        }
    }
}
