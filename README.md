# Wypożyczalnia książek
Aktualne zmiany dokonane w projekcie:
- Możliwość wypożyczenia książki przez stworzonego wcześniej użytkownika
- Dodanie nowego użytkownika poprzez podanie jego imienia, nazwiska i adresu e-mail
- Dodanie ograniczenia na czytelników aby w danym momencie nie mogli mieć wypożyczonych więcej niż 5 książek
- Możliwość zwrotu książki poprzez ówczesne podanie ID czytelnika i zwracanej książki
- Podgląd na liste zbiorów książek
- Aktualizowaną listę dostępnych i wypożyczonych książek
- Aktualizowaną listę tytułów aktualnie wypożyczonych przez danego czytelnika


Wszystkie te dane są na bieżąco aktualizowane w stworzonej bazie danych w programie SQL Server Management, gdzie wykonany skrypt SQL tworzy strukturę bazy danych dla biblioteki. Skrypt tworzy cztery tabele: "Authors" (autorzy), "Customers" (klienci), "Books" (książki) i "Genres" (gatunki). Ponadto, tworzone są dwie dodatkowe tabele: "BooksGenres" (łącznik między książkami a gatunkami) oraz "BooksCustomers" (łącznik między książkami a klientami).


Planowane zmiany(lista ta będzie cały czas aktualizowana):
- Dodanie do każdej książki jej rodzaj gatunku
- Możliwość dodawania nowych książek
