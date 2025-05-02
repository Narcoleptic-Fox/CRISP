namespace CRISP.Tests.Commands
{
    public class UpdateCommandTests
    {
        [Fact]
        public void UpdateCommand_Constructor_SetsId()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            TestUpdateCommand command = new() { Id = id };

            // Assert
            command.Id.ShouldBe(id);
        }

        [Fact]
        public void UpdateCommand_InheritsFromCommand()
        {
            // Arrange
            TestUpdateCommand command = new();

            // Assert
            command.ShouldBeAssignableTo<Command>();
            command.ShouldBeAssignableTo<IRequest<Response>>();
        }

        // Concrete implementation of UpdateCommand for testing
        private record TestUpdateCommand : UpdateCommand<Guid>
        {
            // Additional properties for testing can be added here
        }
    }
}