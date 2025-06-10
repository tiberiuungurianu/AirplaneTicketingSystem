Airplane Ticketing System

This project is a complete console-based airplane seat allocation application developed in C# using the .NET framework (Visual Studio 2022).
It allows users to book, change, and manage seats on a plane, optimising seat allocation based on passenger preferences.

Features:

Object-oriented design: The core logic is encapsulated in dedicated classes.
Book First Class or Economy seats with real-time availability checks.
Change seat within the chosen class if the allocated seat is not suitable.
Special services: Handles special needs, dietary requirements, and additional services for passengers.
Seating display: Visualise seating by seat number or passenger name.
Persistent seat state: The seating arrangement is saved and reloaded automatically via a .txt file.
Input validation: Guides users and prevents invalid data entries.
Getting Started

Prerequisites:

Visual Studio 2022 (Community Edition is sufficient)
.NET Framework 6.0 or newer

How to run:

Open the .sln file in Visual Studio 2022.
Build and run the solution (press F5 or use the Start button).
Interact with the console interface to book, change, or review seats.
The application automatically creates and manages a text file to save seat allocations between runs.

Architecture:

SimpleAirplaneSeating class: Handles all seat management, booking logic, special services and data persistence.
Program class: Contains the Main entry point and starts the application.

Technologies Used:

C#
.NET Framework
Standard .NET libraries: System, System.Collections.Generic, System.IO, System.Linq, and more
