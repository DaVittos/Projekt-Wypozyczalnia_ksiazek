using System;
using System.Data.SqlClient;

namespace LibraryApp
{
    class Program
    {
        static string connectionString = "Data Source=DKOMPUTER\\WINCC;Initial Catalog=MyLibrary;Integrated Security=True";

        static void Main(string[] args)
        {
            Console.WriteLine("Witaj w aplikacji wypożyczalni książek!");

            while (true)
            {
                Console.WriteLine("Wybierz opcję:");
                Console.WriteLine("1. Wyświetl listę książek");
                Console.WriteLine("2. Wypożycz książkę");
                Console.WriteLine("3. Zwróć książkę");
                Console.WriteLine("4. Wyświetl listę czytelników z wypożyczonymi książkami");
                Console.WriteLine("5. Dodaj czytelnika");
                Console.WriteLine("0. Wyjdź z aplikacji");

                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        DisplayBooks();
                        Console.WriteLine("Naciśnij 1, aby powrócić do głównego menu.");
                        Console.ReadLine();
                        break;
                    case "2":
                        BorrowBook();
                        Console.WriteLine("Naciśnij 1, aby powrócić do głównego menu.");
                        Console.ReadLine();
                        break;
                    case "3":
                        ReturnBook();
                        Console.WriteLine("Naciśnij 1, aby powrócić do głównego menu.");
                        Console.ReadLine();
                        break;
                    case "4":
                        DisplayReadersWithBorrowedBooks();
                        Console.WriteLine("Naciśnij 1, aby powrócić do głównego menu.");
                        Console.ReadLine();
                        break;
                    case "5":
                        AddReader();
                        Console.WriteLine("Naciśnij 1, aby powrócić do głównego menu.");
                        Console.ReadLine();
                        break;
                    case "0":
                        Console.WriteLine("Dziękujemy za skorzystanie z aplikacji!");
                        return;
                    default:
                        Console.WriteLine("Nieprawidłowa opcja. Spróbuj ponownie.");
                        break;
                }

                Console.WriteLine();
            }
        }


        static void DisplayBooks()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT B.Id, B.Title, A.Name AS Author, B.PublicationYear, B.Quantity, B.BorrowedQuantity FROM Books B INNER JOIN Authors A ON B.AuthorId = A.Id";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                Console.WriteLine("Lista książek:");
                Console.WriteLine("{0,-5} | {1,-30} | {2,-20} | {3,-15} | {4,-10} | {5,-12}", "ID", "Tytuł", "Autor", "Rok publikacji", "Dostępne", "Wypożyczone");
                Console.WriteLine("-------------------------------------------------------");

                while (reader.Read())
                {
                    int bookId = (int)reader["Id"];
                    string title = (string)reader["Title"];
                    string author = (string)reader["Author"];
                    int publicationYear = (int)reader["PublicationYear"];
                    int quantity = (int)reader["Quantity"];
                    int borrowedQuantity = (int)reader["BorrowedQuantity"];

                    Console.WriteLine("{0,-5} | {1,-30} | {2,-20} | {3,-15} | {4,-10} | {5,-12}",
                        bookId, title.PadRight(30), author.PadRight(20), publicationYear, quantity, borrowedQuantity);
                }

                reader.Close();
            }
        }

        static void BorrowBook()
        {
            Console.WriteLine("Dostępni czytelnicy:");

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Id, FirstName, LastName FROM Customers";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                Console.WriteLine("{0,-5} | {1,-15} | {2,-15}", "ID", "Imię", "Nazwisko");
                Console.WriteLine("---------------------------------");

                while (reader.Read())
                {
                    int customerId = (int)reader["Id"];
                    string firstName = (string)reader["FirstName"];
                    string lastName = (string)reader["LastName"];

                    Console.WriteLine("{0,-5} | {1,-15} | {2,-15}", customerId, firstName, lastName);
                }

                reader.Close();
            }

            Console.WriteLine();

            Console.WriteLine("Podaj ID czytelnika, który wypożycza książkę:");
            int selectedCustomerId = int.Parse(Console.ReadLine());

            Console.WriteLine();

            DisplayBooks();

            Console.WriteLine();
            Console.WriteLine("Podaj ID książki, którą chcesz wypożyczyć:");
            int selectedBookId = int.Parse(Console.ReadLine());

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkAvailabilityQuery = "SELECT Quantity, BorrowedQuantity FROM Books WHERE Id = @BookId";

                SqlCommand checkAvailabilityCommand = new SqlCommand(checkAvailabilityQuery, connection);
                checkAvailabilityCommand.Parameters.AddWithValue("@BookId", selectedBookId);

                connection.Open();

                SqlDataReader availabilityReader = checkAvailabilityCommand.ExecuteReader();

                if (availabilityReader.Read())
                {
                    int quantity = (int)availabilityReader["Quantity"];
                    int borrowedQuantity = (int)availabilityReader["BorrowedQuantity"];

                    if (quantity - borrowedQuantity > 0)
                    {
                        string checkBorrowedBooksQuery = "SELECT COUNT(*) FROM Books WHERE CurrentCustomerId = @CustomerId";

                        SqlCommand checkBorrowedBooksCommand = new SqlCommand(checkBorrowedBooksQuery, connection);
                        checkBorrowedBooksCommand.Parameters.AddWithValue("@CustomerId", selectedCustomerId);

                        availabilityReader.Close();

                        int borrowedBooksCount = (int)checkBorrowedBooksCommand.ExecuteScalar();

                        if (borrowedBooksCount < 5)
                        {
                            string borrowQuery = "UPDATE Books SET BorrowedQuantity = BorrowedQuantity + 1, CurrentCustomerId = @CustomerId WHERE Id = @BookId";

                            SqlCommand borrowCommand = new SqlCommand(borrowQuery, connection);
                            borrowCommand.Parameters.AddWithValue("@CustomerId", selectedCustomerId);
                            borrowCommand.Parameters.AddWithValue("@BookId", selectedBookId);

                            int rowsAffected = borrowCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                Console.WriteLine("Książka została wypożyczona.");

                                // Aktualizacja liczby dostępnych i wypożyczonych książek w bazie danych
                                string updateQuantityQuery = "UPDATE Books SET Quantity = Quantity - 1 WHERE Id = @BookId";
                                SqlCommand updateQuantityCommand = new SqlCommand(updateQuantityQuery, connection);
                                updateQuantityCommand.Parameters.AddWithValue("@BookId", selectedBookId);
                                updateQuantityCommand.ExecuteNonQuery();
                            }
                            else
                            {
                                Console.WriteLine("Nie udało się wypożyczyć książki. Sprawdź poprawność ID książki i ID czytelnika.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Nie można wypożyczyć więcej niż 5 książek naraz.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Nie można wypożyczyć tej książki. Brak dostępnych egzemplarzy.");
                    }
                }
                else
                {
                    Console.WriteLine("Nie można znaleźć książki o podanym ID.");
                }

                availabilityReader.Close();
            }
        }

        static void ReturnBook()
        {
            Console.WriteLine("Podaj ID czytelnika, który zwraca książkę:");
            int customerId = int.Parse(Console.ReadLine());

            Console.WriteLine();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string checkBorrowedBooksQuery = "SELECT B.Id, B.Title FROM Books B WHERE B.CurrentCustomerId = @CustomerId";

                SqlCommand checkBorrowedBooksCommand = new SqlCommand(checkBorrowedBooksQuery, connection);
                checkBorrowedBooksCommand.Parameters.AddWithValue("@CustomerId", customerId);

                connection.Open();

                SqlDataReader borrowedBooksReader = checkBorrowedBooksCommand.ExecuteReader();

                if (borrowedBooksReader.HasRows)
                {
                    Console.WriteLine("Wypożyczone książki:");
                    Console.WriteLine("{0,-5} | {1,-30}", "ID", "Tytuł");
                    Console.WriteLine("---------------------------------------");

                    while (borrowedBooksReader.Read())
                    {
                        int bookId = (int)borrowedBooksReader["Id"];
                        string title = (string)borrowedBooksReader["Title"];

                        Console.WriteLine("{0,-5} | {1,-30}", bookId, title);
                    }

                    borrowedBooksReader.Close();

                    Console.WriteLine();
                    Console.WriteLine("Podaj ID książki, którą chcesz zwrócić:");
                    int bookIdToReturn = int.Parse(Console.ReadLine());

                    string returnQuery = "UPDATE Books SET BorrowedQuantity = BorrowedQuantity - 1, CurrentCustomerId = NULL WHERE Id = @BookId AND CurrentCustomerId = @CustomerId";

                    SqlCommand returnCommand = new SqlCommand(returnQuery, connection);
                    returnCommand.Parameters.AddWithValue("@BookId", bookIdToReturn);
                    returnCommand.Parameters.AddWithValue("@CustomerId", customerId);

                    int rowsAffected = returnCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        Console.WriteLine("Książka została zwrócona.");

                        // Aktualizacja liczby dostępnych i wypożyczonych książek w bazie danych
                        string updateQuantityQuery = "UPDATE Books SET Quantity = Quantity + 1 WHERE Id = @BookId";
                        SqlCommand updateQuantityCommand = new SqlCommand(updateQuantityQuery, connection);
                        updateQuantityCommand.Parameters.AddWithValue("@BookId", bookIdToReturn);
                        updateQuantityCommand.ExecuteNonQuery();
                    }
                    else
                    {
                        Console.WriteLine("Nie udało się zwrócić książki. Sprawdź poprawność ID książki.");
                    }
                }
                else
                {
                    Console.WriteLine("Czytelnik nie ma wypożyczonych książek.");
                }

                borrowedBooksReader.Close();
            }
        }

        static void DisplayReadersWithBorrowedBooks()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT C.Id, C.FirstName, C.LastName, C.Email, B.Title FROM Customers C LEFT JOIN Books B ON C.Id = B.CurrentCustomerId";

                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                Console.WriteLine("Lista czytelników z wypożyczonymi książkami:");
                Console.WriteLine("{0,-5} | {1,-15} | {2,-15} | {3,-30} | {4,-30}", "ID", "Imię", "Nazwisko", "E-mail", "Wypożyczone książki");
                Console.WriteLine("--------------------------------------------------------------------------");

                while (reader.Read())
                {
                    int customerId = (int)reader["Id"];
                    string firstName = (string)reader["FirstName"];
                    string lastName = (string)reader["LastName"];
                    string email = (string)reader["Email"];
                    string title = reader.IsDBNull(4) ? "" : (string)reader["Title"];

                    Console.WriteLine("{0,-5} | {1,-15} | {2,-15} | {3,-30} | {4,-30}",
                        customerId, firstName, lastName, email, title);
                }

                reader.Close();
            }
        }

        static void AddReader()
        {
            Console.WriteLine("Podaj imię czytelnika:");
            string firstName = Console.ReadLine();

            Console.WriteLine("Podaj nazwisko czytelnika:");
            string lastName = Console.ReadLine();

            Console.WriteLine("Podaj adres e-mail czytelnika:");
            string email = Console.ReadLine();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string insertReaderQuery = "INSERT INTO Customers (FirstName, LastName, Email) VALUES (@FirstName, @LastName, @Email)";

                SqlCommand insertReaderCommand = new SqlCommand(insertReaderQuery, connection);
                insertReaderCommand.Parameters.AddWithValue("@FirstName", firstName);
                insertReaderCommand.Parameters.AddWithValue("@LastName", lastName);
                insertReaderCommand.Parameters.AddWithValue("@Email", email);

                connection.Open();

                int rowsAffected = insertReaderCommand.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    Console.WriteLine("Czytelnik został dodany do bazy danych.");
                }
                else
                {
                    Console.WriteLine("Nie udało się dodać czytelnika do bazy danych.");
                }
            }
        }
    }
}