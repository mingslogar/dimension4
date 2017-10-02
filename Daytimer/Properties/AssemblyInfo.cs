using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(GlobalAssemblyInfo.AssemblyName)]
[assembly: AssemblyDescription("The next generation of time management software.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(GlobalAssemblyInfo.AssemblyCompany)]
[assembly: AssemblyProduct(GlobalAssemblyInfo.AssemblyName)]
[assembly: AssemblyCopyright(GlobalAssemblyInfo.AssemblyCopyright)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(true)]

[assembly: Guid("90EACF9B-A955-48E9-8657-A4CD3B0E6C5E")]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
	ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
	//(used if a resource is not found in the page, 
	// or application resource dictionaries)
	ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
	//(used if a resource is not found in the page, 
	// app, or any theme specific resource dictionaries)
)]


// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(GlobalAssemblyInfo.AssemblyVersion)]

[assembly: DependencyAttribute("AddressParser", LoadHint.Sometimes)]
[assembly: DependencyAttribute("CharMap", LoadHint.Sometimes)]
[assembly: DependencyAttribute("colorpickerlib", LoadHint.Sometimes)]
[assembly: DependencyAttribute("CoreAudioApi", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.Backgrounds", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Controls", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.DatabaseHelpers", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Dialogs", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.DockableDialogs", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Fonts", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Functions", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Fundamentals", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.GoogleCalendarHelpers", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.GoogleMapHelpers", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.Help", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.ICalendarHelpers", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Images", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.MailHelpers", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.Search", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.Styles", LoadHint.Always)]
[assembly: DependencyAttribute("Daytimer.Toasts", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Daytimer.WikiQuoteHelper", LoadHint.Always)]
[assembly: DependencyAttribute("DDay.Collections", LoadHint.Always)]
[assembly: DependencyAttribute("DDay.iCal", LoadHint.Always)]
[assembly: DependencyAttribute("Demo.Main", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Gma.UserActivityMonitor", LoadHint.Sometimes)]
[assembly: DependencyAttribute("Microsoft.Windows.Shell", LoadHint.Always)]
[assembly: DependencyAttribute("Modern.FileBrowser", LoadHint.Sometimes)]
[assembly: DependencyAttribute("RecurrenceGenerator", LoadHint.Sometimes)]
[assembly: DependencyAttribute("RibbonControlsLibrary", LoadHint.Always)]
[assembly: DependencyAttribute("Thought.vCards", LoadHint.Sometimes)]
