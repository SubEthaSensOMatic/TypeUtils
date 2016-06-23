using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    public class Trillian2
    {
        public string A { get; set; }
        public double B { get; set; }
        public double C { get; set; }
        public string E { get; set; }
    }

    [TestClass]
    public class MapperTest
    {
        public static IPropertyMapper _mapper;
        public static IPropertyMapper _mapperNoConversionNeeded;

        [ClassInitialize]
        public static void initClass(TestContext testContext)
        {
            var mapping = new Mapping<Trillian, Marvin>()
                .map("A", "A")
                .map("B", "B")
                .map("C", "C")
                .map("E", "E");

            _mapper = new PropertyMapperFactory()
                .createPropertyMapper(mapping);

            var mapping2 = new Mapping<Trillian, Trillian2>()
                .map("A")
                .map("B")
                .map("C")
                .map("E");

            _mapperNoConversionNeeded = new PropertyMapperFactory().createPropertyMapper(mapping2);
        }


        [TestMethod]
        public void TestSimpleObjectMapping()
        {
            var source = new Trillian()
            {
                A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000",
                B = 1234.567,
                C = 42735.9998842593,
                E = "true"
            };

            var target = new Marvin();

            _mapper.map(source, target);

            Assert.AreEqual(Guid.Parse("BE5B1F80-B878-44B4-8B61-DEADBEEF0000"), target.A);
            Assert.AreEqual(1235, target.B);
            Assert.AreEqual(new DateTime(2016, 12, 31, 23, 59, 50), target.C);
            Assert.AreEqual(true, target.E);
        }

        [TestMethod]
        public void Performance()
        {
            var source = new Trillian();
            source.A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000";
            source.B = 1234.567;
            source.C = 42735.9998842593;
            source.E = "true";

            for (int i = 0; i < 1000000; i++)
            {
                var target = new Marvin();
                _mapper.map(source, target);

            }
        }

        [TestMethod]
        public void PerformanceNoConversion()
        {
            var source = new Trillian();
            source.A = "BE5B1F80-B878-44B4-8B61-DEADBEEF0000";
            source.B = 1234.567;
            source.C = 42735.9998842593;
            source.E = "true";

            for (int i = 0; i < 1000000; i++)
            {
                var target = new Trillian2();
                _mapperNoConversionNeeded.map(source, target);
            }
        }
    }
}
