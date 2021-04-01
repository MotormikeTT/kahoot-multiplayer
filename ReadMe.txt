Cards basic WCF example #3 (COMPLETE) - INFO-5060, Winter 2021
--------------------------------------------------------------

Mar 23, 2021

Example:	Using Card as a data contract and administrative endpoints
			defined in the App.config files (WCF example #2)

This WCF example is now COMPLETE. It defines a new type of service contract, 
often referred to as a callback contract, called ICallback. This contract is 
implemented by the client's MainWindow class and its UpdateClient() method is 
called by the Shoe service object whenever a live update is needed because 
the state of the Shoe object changed.

How to Run the Example:

1.	Make sure you are running Visual Studio via "Run as Administrator"
2.	Build all projects before testing it.
2.	The server host must be started first and then the client application.
	The solution has been configured so that all this will happen when you 
	click on the "Start" button in the toolbar.
3.	To execte multiple instances of the client it may work better to launch
	the client outside of Visual Studio by double clicking on the file
	CardsGUIClient\bin\Debug\CardsGUIClient.exe.