# Project Variables

| Variable         | Value | Notes                                                              |
| ---------------- | ----- | ------------------------------------------------------------------ |
| ProjectNamespace | PNTG  | Default PNTG Namespace to use for scripts and assembly definitions |

## About the Project
We're creating a simple number tracing game for toddlers. Note that we do want to teach toddlers the correct way of tracing a number (i.e they should make the correct strokes matching what is taught on school) - the core gameplay loop is:
- Show the shape of a number between 0-9 on the background (chosen at random)
- the UI prompts the toddler to make a specific stroke (also indicating the desired directionality) 
- The toddler attempts to make the stroke
	- If the toddler stops the stroke mid-way he/she will have to start again
	- If the toddler moves his/her finger too far away from the desired stroke he/she will have to start again (but there is some margin)
- When the toddler completes the stroke:
	- If the number consists of multiple strokes, he/she is prompted to start the next stroke part of the number - the completed stroke is visualized over the background
	- If the number is completely stroked the toddler receives a point (count shown in top-right corner) - and the next number is loaded
## Unity3D Specific Rules
- Don’t change code outside of the 'Assets' folder; if code outside this folder needs changing first ask confirmation with the user
- When creating files within Unity3D Assets folders don’t create .meta files, but rather let Unity3D create them - ask the UMCP tool to force an update
- If script files cannot be found in the 'Assets' folder, try looking for them in the package cache library at 'Library\PackageCache' - if you still can't find what you're looking for, feel free to ask the user (don't get stuck looking for scriptfiles)
- Do your best to keep the Unity3D Project tidy and clean:
	- Work within a subfolder of the project assets folder, if the user hasn’t specified one, please request the user to provide one
	- Create runtime/application scripts in a ‘scripts/runtime’ subfolder – make sure an assembly definition file is present in this folder, if none exist ask the user for a namespace to use (but please suggest one based on the subfolder)
	- Create editor-only scripts in a ‘scripts/editor’ subfolder – make sure an assembly definition file is present in this folder – and make sure its setup for editor-only, if none exist ask the user for a namespace to use (but please suggest one based on the subfolder)
	- If you create scripts to generate stuff in the editor (e.g. execute menu item) - place them in the 'Assets/GenAITemp' - make sure an editor only assembly definition is present in this folder - and when they’ve served their purpose – please remove them
	- Create scenes in a ‘scenes’ subfolder
	- Create prefabs in a ‘prefabs’ subfolder
	- Create materials in a ‘materials’ subfolder
	- Create textures in a ‘textures’ subfolder
	- Create UIElements/UIToolkit assets in a ‘UIElements’ subfolder
- Consider the serializability and memory persistence of the code you are building:
	- If data needs to be stored as part of the scene and/or as game data (e.g. save files) mark the affected classes with the 'Serializable' attribute
	- If a field is supposed to be serializable, but it is typed abstract or as a parent type, mark it with the 'SerializeReference' attribute
	- If data needs to be transferable between scenes prefer using a scriptable object for storing that data - don't forget to add the CreateAssetMenu attribute to the class inheriting ScriptableObject (example:  '[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/SpawnManagerScriptableObject", order = 1)]')
- When creating UI features please take the following into account
	- Don't use IMGUI for UI, use UIElements/UIToolkit - also when building inspectors and windows for use within the UnityEditor

## Software Architecture Rules
- apply boyscout rule - leave all code better than how you found it - but only refactor if on the critical path of your current assignment
- Add Summaries to all classes and class members you create, make sure to make them descriptive. If you update the functioning of a class member update the summary
	- If you create any complex code sections within a member, be sure to add a short explanatory comment in the line above, or behind
- When architecting code structures (classes, structs), prefer to keep processing logic out of the data (apply functional programming whenever possible)
	- create a data struct with just fields, properties and a optionally a constructor and/or destructor
	- create a static utility class (generally named [datastructname]Utility) that contains all processing logic functions as static functions
		- Be mindfull of accessiblity - if tools are only accessed locally mark them private, or internal if they are targeted by a (unit)test
	- If functions require upwards of 4 parameters to function, create a custom data struct to forward data to that function
- Whenever possible - create tests for the functions you create to validate that they work in an atomic fashion. 
	- please be descriptive in the test functions' summary on what exactly is tested. If possible, use test fixtures and cases for variant testing
	- within the Unity Client project these must be located within the 'UCMP Client Editor Test Folder'. Note that these tests make use of NUnit
	- within the Unity Server project these must be located within the 'UMCP MCP Server Tests' project. 
		- Note that any (integration) tests created should be part of a batched invocation of all (integration) tests, but it should also be invocable individually from the commandline (please include a short readme per test on how to invoke)
		- All integration tests Should access the server tools using mcp interface, not direct class invocation

# logging Rules
- Store data within the chroma databases' in chunks of maximally 512-1024 tokens

## Task Execution flow
- Before starting work:
	- Validate the Chroma tool targets a storage folder local to the current project folder (we don't want to risk polluting other projects) - if its not, please inform and ask input of the user before continuing 
	- make sure the user defined an 'IssueID variable, 
	- make sure a chroma database named 'ProjectDevelopmentLog' exists - if not create it - we'll refer to this database as 'ProjectDB'
	- make sure a chroma database with the 'IssueID' variable value exists - if not create it - we'll refer to this database as 'IssueDB'
	- if the 'IssueDB' exists, please use the content of that database as additional reference for understanding the problem 
	- If the 'ProjectDB' database exists, scan it for issue(s) database(s) that may be related to the current issue and use these as additional reference for understanding the problem - report to the user which issues you have identified as relevant context
- When starting work:
	- Log a summary of your planned approach and steps in the ‘IssueDB’, mark it as ‘Planned Approach’
	- make sure the Unity Client is running in edit mode (Use the UMCP GetUnityClientState tool), if not – wait until it enters this state (unless the step explicitly requires a different operation mode)
	- Use the ‘MarkStartOfNewStep’ UMCP tool to mark the start of a new step – use descriptive names for your steps
- When completing work
	- Always call the ‘ForceUpdateEditor’ tool to make sure all files are properly compiled
	- Use the ‘RequestStepLogs’ tool to check the current steps’ log– if exceptions are encountered since starting the step (likely due to compilation issues), make sure to resolve those first, before continuing
	- use the UMCP 'runTests' tool to rerun all unittests - if the returned log returns any exceptions, make sure to resolve those first, before continuing
	- If your findings resulted in a different plan update the ‘Planned Approach’ section of the ‘IssueDB’ 
	- ·Add a short description of the work undertaken in this step in and mark it as ‘Actions Taken’ section in the 'IssueDB'
	- Add a document on the current 'IssueID' to the 'ProjectDB' - it should describe:
		- A summary of the original task (max 3 sentences)
		- A summary of the resolution (max 3 sentences)
		- A reference to the 'IssueDB'
		- The list of other issues (by ID) that were found relevant