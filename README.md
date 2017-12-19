# Meteo App
Aplikacja umożliwia pobieranie i wyświetlanie prognozy pogody z serwisu [meteo.pl](http://www.meteo.pl). Aplikacja pozwala na dodanie własnych miejsc i aktualizowanie ich w tle do użytku offline.

Prognoza pogody jest generowana przez [ICM - Uniwersytet Warszawski](http://icm.edu.pl).

## Obsługa programu
Główne okno programu:
<p align="center"><img src="https://raw.githubusercontent.com/Nirid/Meteo/master/Meteo/Pictures/Main%20Window.png" /></p>

- Miejsce dla którego jest wyświetlana pogoda można zmienić klikacjąc na nazwę obecnego miejsca pod napisem **Miejsce:** i wybierając nowe miejsce z listy.
- Aby sprawdzić gdzie znajduje się miejsce dla którego jest wyświetlana podgoda należy nacisnąć **Sprawdź miejsce na mapie**. (Uwaga, ze względu na sposób przeliczania pozycji bład pozycji może wynosić do kilkunastu kilometrów.
- Po zaznaczeniu kontrolki **Aktualizuj automatycznie** pogoda będzie aktualizowana i zapisywana gdy tylko będzie dostępny nowy metrogram na stronie [meteo.pl](http://www.meteo.pl) do użytku offline.
- Po naciśnięciu **Ustaw jako domyślne** pogoda dla obecnie wybranego miejsca będzie wyświetlana przy otwarciu programu.
- **Edytuj lokalizacje** umożliwia edycję pozycji i nazwy obecnie wybranej lokalizacji.
- **Usuń lokalizację** usuwa obecnie wybraną lokalizację (Uwaga: nie można usunąć domyślnej lokalizacji)
- **Edytuj kolejność na liście** umożliwia edycję wszystkich lokalizacji i ich kolejności w liście.
- Przycisk pod **Dodaj miejsce:** umożliwia dodawanie nowych miejsc wpisując ręcznie koordynaty miejsca przy wybraniu opcji **Podaj koordynaty** lub poprzez wyszukanie miejsca w geocoding api od Google przy wybraniu opcji **Z nazwy**.
- Przycisk **Odśwież** ręcznie wywołuje aktualizację metrogramów, jeżeli przycisk jest zielony znaczy to, że program ma dostęp do interentu, jeżeli jest czerwony znaczy to, że program nie może się połączyć z interentem.
- Program nie pokazuję się na pasku zadań, aby go wyświetlić należy nacisnąć na ikonę kropli w obszarze powiadomień.
- Przy próbie wyłączenia programu jest on minimalizowany do obszaru powiadomień, aby wyłączyć program należy nacisnąć prawym przyciskiem na ikonę kropli i wybrać opcję **Zamknij program**.
## Instalacja
Program został napisany w Visual Studio, Community Edition.

1. Otwórz Visual Studio
2. Otwórz okno Team Explorer
3. Wybierz opcję Clone
4. Wklej adres: https://github.com/Nirid/Meteo.git
5. Naciśnij Clone
