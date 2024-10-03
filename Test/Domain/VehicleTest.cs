using minimal_api.Domain.Entities;

namespace Test.Domain
{
    [TestClass]
    public class VehicleTest
    {
        [TestMethod]
        public void TestarGetSetPropriedades()
        {
            // Arrange 
            var vehicle = new Vehicle();

            // Act
            vehicle.Id = 1;
            vehicle.Nome = "Ford Ka";
            vehicle.Marca = "Ford";
            vehicle.Ano = 2024;
            
            // Assert
            Assert.AreEqual(1, vehicle.Id);
            Assert.AreEqual("Ford Ka", vehicle.Nome);
            Assert.AreEqual("Ford", vehicle.Marca);
            Assert.AreEqual(2024, vehicle.Ano);
        }
    }
}