using Fcg.Core.Abstractions.Common.Exceptions;
using Fcg.Core.Abstractions.Resources;
using Fcg.Orders.Domain.Entities;
using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace Fcg.Orders.Domain.Tests.Entities
{
    public class GameSnapshotTests
    {
        public string GetDescription() => "Descrição válida";
        public string GetName() => "Jogo Válido";
        public decimal GetPrice() => 100m;
        public string GetGenre() => "Ação";

        [Fact]
        public void Constructor_ShouldCreateGame_WhenNameAndDescriptionAreRight()
        {
            //Act
            string genero = GetGenre(); 
            string descricaoJogo = GetDescription();
            string nomeJogo = GetName();
            decimal price = GetPrice(); 
            //Arrange
            var gameSnapshot = new GameSnapshot(Guid.NewGuid(), nomeJogo, descricaoJogo, genero, price, DateTime.UtcNow);
            //Assert
            Assert.NotNull(gameSnapshot);   
            gameSnapshot.Name.Should().Be("Jogo Válido");
            gameSnapshot.Description.Should().Be("Descrição válida");
        }
        [Fact]
        public void Constructor_ShouldNotCreateGame_WhenNameIsEmpty()
        {
            //Act
            string descricaoJogo = GetDescription();
            string genero = GetGenre();
            decimal price = GetPrice();
            //Arrange
            var exception = Assert.Throws<DomainException>(() => new GameSnapshot(Guid.NewGuid(), string.Empty, descricaoJogo, genero, price, DateTime.UtcNow));
            //Assert
            Assert.Equal(DomainMessages.GameNameRequired, exception.Message);
        }
        [Fact]
        public void Constructor_ShouldNotCreateGame_WhenDescriptionIsEmpty()
        {
            //Act
            string name = GetName();
            string genero = GetGenre();
            decimal price = GetPrice();
            //Arrange
            var exception = Assert.Throws<DomainException>(() => new GameSnapshot(Guid.NewGuid(), name ,string.Empty, genero, price, DateTime.UtcNow));
            //Assert
            Assert.Equal(DomainMessages.GameDescriptionRequired, exception.Message);
        }
    }
}
