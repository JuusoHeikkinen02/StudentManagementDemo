using Microsoft.EntityFrameworkCore;
using StudentManagement.Data;
using StudentManagement.Models;
using StudentManagement.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace TestProjectStudent
{
    public class StudentControllerTest
    {


        [Fact]
        public async Task GetStudents_ReturnsAllStudents()
        {
            // 1. Luodaan DbCOntextOptions
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest")
                .Options;

            //2. LIs‰t‰‰n "Testidataan" muutama opiskelija

            using (var context = new StudentContext(options))
            {
                context.Students.Add(new Student { Id = 2, FirstName = "Maija", LastName = "Meik‰l‰inen", Age = 20 });
                context.Students.Add(new Student { Id = 3, FirstName = "Matti", LastName = "Meik‰l‰inen", Age = 22 });
                await context.SaveChangesAsync();
            }

            // 3. Suoritetaan varsinainen testi uudella Context-instanssilla
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                var result = await controller.GetStudents();

                // 4. Tarkistetaan, ett‰ tuloksena on kaikki oppilaat (2 kpl)
                var actionResult = Assert.IsType<ActionResult<IEnumerable<Student>>>(result);
                var studentsList = Assert.IsAssignableFrom<IEnumerable<Student>>(actionResult.Value);

                Assert.Equal(2, studentsList.Count());
            }

        }


        [Fact]
        public async Task GetStudent_ReturnsNotFound_WhenStudentDoesNotExist()
        {
            // InMemory -kanta luodaan
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_NotFound")
                .Options;

            // Ei lis‰t‰ dataa, jotta opiskelijaa ei lˆydy
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);
                var result = await controller.GetStudent(99); // 99 puuttuu

                Assert.IsType<NotFoundResult>(result.Result);
            }
        }

        [Fact]
        public async Task PostStudentsTest()
        {
            // 1. Luodaan DbCOntextOptions
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Post")
                .Options;
            //Arrange
            var context = new StudentContext(options);
            var controller = new StudentsController(context);
            var newStudent = new Student { Id = 1, FirstName = "John", LastName = "Doe", Age = 22 };

            //Act 
            var result = await controller.PostStudent(newStudent);
            var studentInDb = await context.Students.FindAsync(1);

            //Assert
            Assert.NotNull(studentInDb);
            Assert.Equal("John", studentInDb.FirstName);
            Assert.Equal(22, studentInDb.Age);



        }

        [Fact]
        public async Task DeleteStudentsTest()
        {
            // 1. Luodaan DbCOntextOptions
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Delete")
                .Options;
            //Arrange
            var context = new StudentContext(options);
            var controller = new StudentsController(context);
            var newStudent = new Student { Id = 4, FirstName = "John", LastName = "Doe", Age = 22 };

            //Act : Lis‰t‰‰n ensiksi uusi student
            await controller.PostStudent(newStudent);
            var studentInDb = await context.Students.FindAsync(4);


            //Assert: Etsit‰‰n lis‰tty student ennen poistoa
            Assert.NotNull(studentInDb);
            Assert.Equal("John", studentInDb.FirstName);
            Assert.Equal(22, studentInDb.Age);

            // Act: poistetaan student
            var deleteResult = await controller.DeleteStudent(4);

            // Assert: Etsit‰‰n poistettua studenttia
            var deletedStudent = await context.Students.FindAsync(4);
            Assert.Null(deletedStudent);



        }

        [Fact]
        public async Task PutController_StudentsTest()
        {
            // 1. Luodaan DbCOntextOptions
            var options = new DbContextOptionsBuilder<StudentContext>()
                .UseInMemoryDatabase(databaseName: "StudentDbTest_Put")
                .Options;

            //Test commit 2
            using (var context = new StudentContext(options))
            {
                var controller = new StudentsController(context);

                //Lis‰t‰‰n InMemoryyn yksi Opiskelija
                var student = new Student { Id = 1, FirstName = "John", LastName = "Doe", Age = 22 };
                context.Students.Add(student);
                await context.SaveChangesAsync();

                // Act: Muokataan nykyist‰ Opiskelijaa
                var studentToUpdate = await context.Students.FindAsync(1);
                studentToUpdate.FirstName = "Johnny";
                studentToUpdate.Age = 23;

                var result = await controller.PutStudent(1, studentToUpdate);

                // Assert
                Assert.IsType<NoContentResult>(result);

                // Tarkistetaan, ett‰ opiskelija on muuttunut
                var studentInDb = await context.Students.FindAsync(1);
                Assert.NotNull(studentInDb);
                Assert.Equal("Johnny", studentInDb.FirstName);
                Assert.Equal(23, studentInDb.Age);
            }
        }



    }


}