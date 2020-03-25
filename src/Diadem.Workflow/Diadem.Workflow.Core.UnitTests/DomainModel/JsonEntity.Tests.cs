using System;
using Diadem.Core.DomainModel;
using NUnit.Framework;

namespace Diadem.Workflow.Core.UnitTests.DomainModel
{
    public class JsonEntityTests
    {
        private const string Json = @"{
    'title': 'Star Wars',
    'link': 'http://www.starwars.com',
    'description': 'Star Wars blog.',
    'address': {
        'line1': '123 Main St',
        'city': 'Los Angeles',
        'state': 'CA',
        'country': 'USA' 
    },
    'item': []
}";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BlankEntity_AddProperty_Success()
        {
            var jsonEntity = new JsonEntity("Blank", "1", "{}");

            Assert.Throws<Exception>(() => jsonEntity.GetProperty<string>("code"));
            jsonEntity.SetProperty("code", "blank");
            Assert.IsTrue(string.Equals(jsonEntity.GetProperty<string>("code"), "blank", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void BlankEntity_AddEntity_Success()
        {
            var jsonEntityMaster = new JsonEntity("Master", "1");

            var jsonEntityDetailSet = new JsonEntity("Detail", "1", "{'code':'6A8EC675-1647-470E-90FE-6F6B1E426C05'}");

            Assert.Throws<Exception>(() => jsonEntityMaster.GetProperty<string>("detail"));
            jsonEntityMaster.SetEntity("detail", jsonEntityDetailSet);

            var jsonEntityDetailGet = jsonEntityMaster.GetEntity("detail");
            Assert.IsFalse(jsonEntityDetailSet == jsonEntityDetailGet);
            Assert.IsTrue(string.Equals(jsonEntityDetailSet.GetProperty<string>("code"), jsonEntityDetailGet.GetProperty<string>("code")));
        }

        [Test]
        public void Write_Values_Success()
        {
            var jsonEntityChannel = new JsonEntity("Channel", "1", Json);

            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("link"), "http://www.starwars.com", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("description"), "Star Wars blog.", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("title"), "Star Wars", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(jsonEntityChannel.GetCollection("item").Count == 0);

            jsonEntityChannel.SetProperty("link", "http://www.starwars.org");
            jsonEntityChannel.SetProperty("description", "Star Wars best blog.");
            jsonEntityChannel.SetProperty("title", "Star Wars is the best");

            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("link"), "http://www.starwars.org", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("description"), "Star Wars best blog.", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("title"), "Star Wars is the best", StringComparison.OrdinalIgnoreCase));


            var jsonEntityAddress = jsonEntityChannel.GetEntity("address");

            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("line1"), "123 Main St", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("city"), "Los Angeles", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("state"), "CA", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("country"), "USA", StringComparison.OrdinalIgnoreCase));

            jsonEntityAddress.SetProperty("line1", "123 Strip St");
            jsonEntityAddress.SetProperty("city", "Las Vegas");
            jsonEntityAddress.SetProperty("state", "NV");

            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("line1"), "123 Strip St", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("city"), "Las Vegas", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("state"), "NV", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Read_Values_Success()
        {
            var jsonEntityChannel = new JsonEntity("Channel", "1", Json);

            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("link"), "http://www.starwars.com", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("description"), "Star Wars blog.", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityChannel.GetProperty<string>("title"), "Star Wars", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(jsonEntityChannel.GetCollection("item").Count == 0);

            var jsonEntityAddress = jsonEntityChannel.GetEntity("address");

            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("line1"), "123 Main St", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("city"), "Los Angeles", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("state"), "CA", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(string.Equals(jsonEntityAddress.GetProperty<string>("country"), "USA", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Read_Values_MissingProperty()
        {
            var jsonEntityChannel = new JsonEntity("Channel", "1", Json);

            Assert.Throws<Exception>(() => jsonEntityChannel.GetCollection("missing"));
            Assert.Throws<Exception>(() => jsonEntityChannel.GetEntity("missing"));
            Assert.Throws<Exception>(() => jsonEntityChannel.GetProperty<string>("missing"));
        }

        [Test]
        public void Read_Values_WrongType()
        {
            var jsonEntityChannel = new JsonEntity("Channel", "1", Json);

            Assert.Throws<Exception>(() => jsonEntityChannel.GetEntity("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<byte>("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<char>("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<decimal>("link"));
            Assert.Throws<InvalidCastException>(() => jsonEntityChannel.GetProperty<Guid>("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<int>("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<short>("link"));
            Assert.Throws<FormatException>(() => jsonEntityChannel.GetProperty<uint>("link"));
        }
    }
}