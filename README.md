# Functionator
Visual studio extension to track the Azure Functions execution order.

It's intended to be used by teams that strongly rely on Azure Durable Functions and are having a hard time tracking their usage and call hierarchy.

# Short description
It's like the Visual Studio's "Find Usages (Shift+F12)" option, that we all commonly use - only for Azure Functions

Activated through text editor's context menu, by selecting the name of the Azure Function and clicking on "The Functionator" option.

Based on the file where the functionality has been activated, it finds usages of the selected Azure Function and displays them using hierarchical view (in form of tree view).

From the hierarchical view, it is possible to navigate to the Azure Function's usage (as displayed in the hierarchy) or to it's definition.

# How it works
Text analyzer that checks .cs files of a project.
Finds everything that fits into Azure Functions definition or usage pattern.
E.g. Every function is decorated with *FunctionName* attribute (the definition), and in itself, it can call other functions using e.g. context.CallActivity, context.CallSubOrchestrator (the usages).
This information is used to build the call hierarchy.
