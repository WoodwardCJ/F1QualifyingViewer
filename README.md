# F1 Qualifying Lap Viewer

A .NET 8 console application for analysing Formula 1 qualifying
lap data from the TracingInsights dataset.

## Requirements

- Visual Studio 2022
- .NET 8 SDK

## Dataset

The application expects the TracingInsights
`session_laptimes.json` file.

Example:

Australian Grand Prix / Qualifying / session_laptimes.json

The application only needs the local JSON file.

## Running the application

Open:

    QualifyingLapViewer.sln

in Visual Studio.

Press:

    F5

or:

    Ctrl + F5

When prompted, enter the full path to:

    session_laptimes.json

For example:

    C:\F1Data\Australian Grand Prix\Qualifying\session_laptimes.json

## Driver lookup

After loading the dataset, enter a driver abbreviation:

    HUL

or:

    BOR

Driver codes are case-insensitive.

## Classification

Enter:

    classification

to display the complete calculated qualifying classification.

## Exit

Enter:

    exit

## Valid lap logic

A lap is considered valid when:

- it has a lap time
- the lap time is greater than zero
- the `del` field is not true

The fastest valid lap is then selected for Q1, Q2 and Q3.

## Qualifying classification

The final classification is calculated as:

1. Drivers participating in Q3
2. Drivers eliminated in Q2
3. Drivers eliminated in Q1

Within each qualifying segment, drivers are ordered by their
fastest valid lap.

## JSON structure

The TracingInsights file is column-oriented rather than a normal
array of lap objects.

For example, the following fields are used:

    time
    lap
    drv
    dNum
    team
    qs
    del
    pb

`qs` identifies the qualifying segment:

    Q1
    Q2
    Q3

`del` indicates whether the lap was deleted.

`pb` indicates whether the lap is the driver's official personal
best.

Additional fields in the source JSON are ignored because they are
not required for this application.

## Example

For the 2026 Australian Grand Prix:

    Driver: BOR
    Team:   Audi
    Number: 5

    Q1    1:20.495
    Q2    1:20.221
    Q3    -

    Final qualifying position: P10

For HUL:

    Driver: HUL
    Team:   Audi
    Number: 27

    Q1    1:21.024
    Q2    1:20.303
    Q3    -

    Final qualifying position: P11

# Technical Write-up 

I used the following prompt in ChatGPT to scaffold the initial implementation:

    I would like to create a c# console application, with a proper class structure, that:

    - Reads the JSON dataset from a local file path 
    - Accept user input specifying a driver (e.g. HUL, BOR) 
    - Present the selected driver’s best valid lap time from each of the three sessions (Q1, Q2, Q3), and final classified qualifying position based upon the dataset 
    
    An example of the json file can be found here: https://github.com/TracingInsights/2026/blob/main/Australian%20Grand%20Prix/Qualifying/session_laptimes.json
    A full reference file of the josn file can be found here: https://github.com/TracingInsights/2026/blob/main/data_dictionary.json

    the final output should be a complete visual studio ready solution

After reviewing the output and tweaking the prompt when necessary I created a VS solution from the output.  Once the VS solution was up and running I tested using the example file and made sure I was happy with the response.

A future enhacement would be to add a repository layer so you could adstract the data source away from the classification code.  This would allow easier externsion of the program to introduce new data source e.g. a database to read the data from with less rework of other aspects of the code base.