
#region System + MSFT 

global using System;
global using System.Collections;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.ComponentModel;
global using System.Diagnostics;
global using System.Globalization;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using System.Runtime.InteropServices;
global using System.Threading;
global using System.Threading.Tasks;
global using System.Windows.Input;

global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

#endregion System 

#region Avalonia 

global using Avalonia;
global using Avalonia.Controls;
global using Avalonia.Controls.ApplicationLifetimes;
global using Avalonia.Controls.Primitives;
global using Avalonia.Controls.Shapes;
global using Avalonia.Data;
global using Avalonia.Data.Converters;
global using Avalonia.Data.Core.Plugins;
global using Avalonia.Input;
global using Avalonia.Input.Platform;
global using Avalonia.Interactivity;
global using Avalonia.Markup.Xaml;
global using Avalonia.Markup.Xaml.Styling;
global using Avalonia.Media;
global using Avalonia.Media.Imaging;
global using Avalonia.Media.Immutable;
global using Avalonia.Platform;
global using Avalonia.Platform.Storage;
global using Avalonia.Threading;

#endregion Avalonia 

#region Framework 

global using Lyt.Avalonia.Interfaces;
global using Lyt.Avalonia.Interfaces.Localization;
global using Lyt.Avalonia.Interfaces.Logger;
global using Lyt.Avalonia.Interfaces.Messenger;
global using Lyt.Avalonia.Interfaces.Model;
global using Lyt.Avalonia.Interfaces.Profiler;
global using Lyt.Avalonia.Interfaces.Random;
global using Lyt.Avalonia.Interfaces.UserInterface;

global using Lyt.Avalonia.Controls;
global using Lyt.Avalonia.Controls.Glyphs;

global using Lyt.Avalonia.Mvvm;
global using Lyt.Avalonia.Mvvm.Animations;
global using Lyt.Avalonia.Mvvm.Core;
global using Lyt.Avalonia.Mvvm.Dialogs;
global using Lyt.Avalonia.Mvvm.Logging;
global using Lyt.Avalonia.Mvvm.Toasting;
global using Lyt.Avalonia.Mvvm.Utilities;

global using Lyt.Avalonia.Localizer;
global using Lyt.Avalonia.Model;
global using Lyt.Avalonia.Persistence;

global using Lyt.Utilities.Extensions;
global using Lyt.Utilities.Profiling;
global using Lyt.Utilities.Randomizing;
global using Lyt.Messaging;

#endregion Framework 


global using Lyt.Avalonia.Translator.Model;
global using Lyt.Avalonia.Translator.Model.DataObjects;
global using Lyt.Avalonia.Translator.Model.Messaging;

global using Lyt.Avalonia.Translator.Messaging;
global using Lyt.Avalonia.Translator.Service;
global using Lyt.Avalonia.Translator.Shell;

global using Lyt.Avalonia.Translator.Workflow;
global using Lyt.Avalonia.Translator.Workflow.CreateNew;
global using Lyt.Avalonia.Translator.Workflow.Interactive;
global using Lyt.Avalonia.Translator.Workflow.Projects;
global using Lyt.Avalonia.Translator.Workflow.RunProject;
global using Lyt.Avalonia.Translator.Workflow.Shared;


//global using Lyt.Avalonia.Translator.Workflow.Intro;
//global using Lyt.Avalonia.Translator.Workflow.Language;
//global using Lyt.Avalonia.Translator.Workflow.Settings;
//global using Lyt.Avalonia.Translator.Workflow.Shared;
