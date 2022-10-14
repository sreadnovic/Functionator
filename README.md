![repo state](https://github.com/sreadnovic/Functionator/actions/workflows/build.yaml/badge.svg)

# Functionator
Visual studio extension to track the Azure Functions execution order.

It's intended to be used by teams that strongly rely on Azure Durable Functions and are having a hard time tracking their usage and call hierarchy.

# Short description
It's like the Visual Studio's "Find Usages (Shift+F12)" option, that we all commonly use - only for Azure Functions

Activated through text editor's context menu, by selecting the name of the Azure Function and clicking on "The Functionator" option.

Based on the file where the functionality has been activated, it finds usages of the selected Azure Function and displays them using hierarchical view (in form of tree view).

From the hierarchical view, it is possible to navigate to the Azure Function's usage (as displayed in the hierarchy) or to it's definition.

# How it works
This is a text analyzer that checks .cs files of a project.

It finds everything that fits into Azure Functions definition or usage pattern.

E.g. Every function is decorated with *FunctionName* attribute (the definition), and in itself, it can call other functions using e.g. *context.CallActivity*, *context.CallSubOrchestrator* (the usages).

This information is used to build the call hierarchy.

Analysis of one file:

![File analysis diagram](https://user-images.githubusercontent.com/7647183/190891326-cdef78ae-1845-4ae1-af11-f594704be50c.jpg)

# Useful resources

Official Microsoft's Visual Studio extensibility documentation home page:

https://learn.microsoft.com/en-us/visualstudio/extensibility/?view=vs-2022

Docs, samples, tips & tricks

https://www.visualstudioextensibility.com/

https://www.vsixcookbook.com/

Toolkit that accelerates Visual Studio extensions development:

https://github.com/VsixCommunity/Community.VisualStudio.Toolkit
