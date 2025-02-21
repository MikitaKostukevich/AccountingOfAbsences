using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AccountingOfAbsences
{
    public static class DatabaseHelper
    {
        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        public static ObservableCollection<Record> GetRecords()
        {
            using (var context = new AppDbContext())
            {
                context.Database.Log = Console.WriteLine; // Логирование SQL-запросов в консоль
                return new ObservableCollection<Record>(
                    context.Records.Include("Student").Include("Student.Class").ToList()
                );
            }
        }


        public static List<Class> GetClasses()
        {
            using (var context = new AppDbContext())
            {
                return context.Classes.ToList();
            }
        }

        public static List<Student> GetStudentsByClass(int classId)
        {
            using (var context = new AppDbContext())
            {
                return context.Students.Where(s => s.ClassId == classId).ToList();
            }
        }

        public static void AddRecord(Record record)
        {
            using (var context = new AppDbContext())
            {
                context.Records.Add(record);
                context.SaveChanges();
            }
        }

        public static void UpdateRecord(Record record)
        {
            using (var context = new AppDbContext())
            {
                var existingRecord = context.Records.Find(record.Id);
                if (existingRecord != null)
                {
                    existingRecord.Reason = record.Reason;
                    existingRecord.Date = record.Date;
                    existingRecord.Classification = record.Classification;
                    context.SaveChanges();
                }
            }
        }


        public static bool AuthenticateUser(string username, string password)
        {
            try
            {
                string hashedPassword = HashPassword(password);
                using (var context = new AppDbContext())
                {
                    return context.Users.Any(u => u.Username == username && u.Password == hashedPassword);
                }
            }
            catch (Exception ex)
            {
                // Логируем сообщение об ошибке и внутреннее исключение
                Console.WriteLine($"Error during user authentication: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");

                // Пробрасываем исключение дальше, если нужно
                throw;
            }
        }


        public static string GetUserRole(string username)
        {
            using (var context = new AppDbContext())
            {
                var user = context.Users.FirstOrDefault(u => u.Username == username);
                return user?.Role ?? "user"; // Возвращает "user", если пользователь не найден
            }
        }

        public static bool IsUserExists(string username)
        {
            using (var context = new AppDbContext())
            {
                return context.Users.Any(u => u.Username == username);
            }
        }

        public static bool RegisterUser(string username, string password, string role)
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    // Проверяем, существует ли пользователь с указанным именем
                    if (context.Users.Any(u => u.Username == username))
                    {
                        return false; // Пользователь с таким именем уже существует
                    }

                    // Хэшируем пароль перед сохранением
                    string hashedPassword = HashPassword(password);

                    // Создаём нового пользователя
                    var user = new User
                    {
                        Username = username,
                        Password = hashedPassword,
                        Role = role
                    };

                    context.Users.Add(user); // Добавляем пользователя в базу данных
                    context.SaveChanges();   // Сохраняем изменения

                    return true; // Регистрация прошла успешно
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during registration: {ex.Message}");
                return false; // Произошла ошибка
            }
        }

        public static void AddClass(Class newClass)
        {
            using (var context = new AppDbContext())
            {
                context.Classes.Add(newClass);
                context.SaveChanges();
            }
        }

        public static void AddStudent(Student newStudent)
        {
            using (var context = new AppDbContext())
            {
                context.Students.Add(newStudent);
                context.SaveChanges();
            }
        }

        public static List<Student> GetStudents()
        {
            using (var context = new AppDbContext())
            {
                return context.Students.Include("Class").ToList();
            }
        }

        public static bool TestDatabaseConnection()
        {
            try
            {
                using (var context = new AppDbContext())
                {
                    context.Database.Connection.Open();
                    context.Database.Connection.Close();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }



        public static void DeleteRecord(int recordId)
        {
            using (var context = new AppDbContext())
            {
                var record = context.Records.Find(recordId);
                if (record != null)
                {
                    context.Records.Remove(record);
                    context.SaveChanges();
                }
            }
        }
    }
}
