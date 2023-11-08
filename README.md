# Diki Dictionary Scrapper

Diki Dictionary Scrapper is an application for getting polish-german entries from [Diki](https://www.diki.pl/slownik-niemieckiego) dictionary and formatting them into spreadsheet friendly format (which is also accepted by some online flashcards applications, like [Quizlet](https://quizlet.com/pl)).

The application is not related in any business way with Diki and does not use any api. It uses web scrapping technique to extract translations from publicly available HTML code.

## Download

You can download prebuild executable file from [here](https://github.com/aserwotka/DikiDictionaryScrapper/releases) (choose latest version, expand "Assets" and select *.exe file). No installation is required, because this is single file application.

## Build

You can manually build and run the application using Visual Studio (tested on Visual Studio 2022 Community Edition).
Alternatively, you can prepare standalone executable file by following those steps:
1. Clone the repository into some `<path>` directory.
2. Run powershell. 

    ![image](https://user-images.githubusercontent.com/46385899/230741209-cdf3ef4a-4051-439e-bc89-97b2450f1773.png)

3. Enter GUI project directory in the solution.
   ```
   cd <path>\DikiDictionaryScrapper\GUI\ 
   ```
4. Run the following command.
   ```
   dotnet publish -r win-x64 -p:PublishSingleFile=true --self-contained true -p:IncludeAllContentForSelfExtract=true -c Release -o publish
   ```
5. If the operation is successful, the file named GUI.exe should be found in `<path>\DikiDictionaryScrapper\GUI\publish\`.

## Usage
![manual](https://github.com/aserwotka/DikiDictionaryScrapper/assets/46385899/9d41f207-2afc-4833-890f-d37715e40316)
1. Special characters buttons for input column.
2. Scrolling check box. Enables translation column scrolling to the bottom after tranlation.
3. Download button. Downloads dictionary translations. Downloading is sequentional, so the more input phrases, the longer it will take.
4. Progress bar and status info.
5. Format button. Formats selected translations from translation column into output column.
6. Version info.
7. About button.
8. Input column. Insert here terms you want to translate in either polish or german language. You can mix them. Each terms should start with a new line. Spaces at the beginning and at the end of lines will be trimmed.
9. Rejected phrases column. Here will go phrases for which no translation was found.
10. Translation column. Here will appear translations. Select the one you are looking for each phrase. If not found, it will display empty row for phrase.
11. Output column. It will display spread sheet (for example excel or google sheets) friendly format. You can also use it in flashcards application like Quizlet.

## Credits
- [Info icons](https://www.flaticon.com/free-icons/info) created by Freepik - Flaticon.
- [German flag icons](https://www.flaticon.com/free-icons/german-flag) created by rizal2109 - Flaticon.

## License

[MIT](https://choosealicense.com/licenses/mit/)
