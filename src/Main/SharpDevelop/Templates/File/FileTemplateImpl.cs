﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Xml;
using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Gui;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Templates
{
	internal class TemplateProperty
	{
		string name;
		string localizedName;
		string type;
		string category;
		string description;
		string defaultValue;
		
		public string Name {
			get {
				return name;
			}
		}
		
		public string LocalizedName {
			get {
				return localizedName;
			}
		}
		
		public string Type {
			get {
				return type;
			}
		}
		
		public string Category {
			get {
				return category;
			}
		}
		
		public string Description {
			get {
				return description;
			}
		}
		
		public string DefaultValue {
			get {
				return defaultValue;
			}
		}
		
		public TemplateProperty(XmlElement propertyElement)
		{
			name         = propertyElement.GetAttribute("name");
			localizedName= propertyElement.GetAttribute("localizedName");
			type         = propertyElement.GetAttribute("type");
			category     = propertyElement.GetAttribute("category");
			description  = propertyElement.GetAttribute("description");
			defaultValue = propertyElement.GetAttribute("defaultValue");
		}
	}
	
	internal class TemplateType
	{
		string    name;
		Hashtable pairs = new Hashtable();
		
		public string Name {
			get {
				return name;
			}
		}
		
		public Hashtable Pairs {
			get {
				return pairs;
			}
		}
		
		public TemplateType(XmlElement enumType)
		{
			name = enumType.GetAttribute("name");
			foreach (XmlElement node in enumType.ChildNodes) {
				pairs[node.GetAttribute("name")] = node.GetAttribute("value");
			}
		}
	}
	
	/// <summary>
	/// This class defines and holds the new file templates.
	/// </summary>
	internal class FileTemplateImpl : FileTemplate
	{
		string author       = null;
		string name         = null;
		string category     = null;
		string languagename = null;
		IImage icon         = null;
		string description  = null;
		string wizardpath   = null;
		string defaultName  = null;
		string subcategory  = null;

		bool   newFileDialogVisible = true;
		
		List<FileDescriptionTemplate> files = new List<FileDescriptionTemplate>();
		List<TemplateProperty> properties  = new List<TemplateProperty>();
		List<TemplateType> customTypes = new List<TemplateType>();
		List<ReferenceProjectItem> requiredAssemblyReferences = new List<ReferenceProjectItem>();
		
		XmlElement fileoptions = null;
		Action<FileTemplateResult> actions;
		
		public string Author {
			get {
				return author;
			}
		}
		public override string Name {
			get {
				return name;
			}
		}
		public override string Category {
			get {
				return category;
			}
		}
		public override string Subcategory {
			get {
				return subcategory;
			}
		}
		public string LanguageName {
			get {
				return languagename;
			}
		}
		public override IImage Icon {
			get {
				return icon;
			}
		}
		public override string Description {
			get {
				return description;
			}
		}
		[Obsolete]
		public string WizardPath {
			get {
				return wizardpath;
			}
		}
		public string DefaultName {
			get {
				return defaultName;
			}
		}
		public XmlElement Fileoptions {
			get {
				return fileoptions;
			}
		}
		public bool NewFileDialogVisible {
			get {
				return newFileDialogVisible;
			}
		}
		
		public List<FileDescriptionTemplate> FileDescriptionTemplates {
			get {
				return files;
			}
		}
		
		public List<TemplateProperty> Properties {
			get {
				return properties;
			}
		}
		
		public List<TemplateType> CustomTypes {
			get {
				return customTypes;
			}
		}
		
		public List<ReferenceProjectItem> RequiredAssemblyReferences {
			get { return requiredAssemblyReferences; }
		}
		
		public bool HasProperties {
			get {
				return properties != null && properties.Count > 0;
			}
		}
		
		public FileTemplateImpl(XmlDocument doc, IReadOnlyFileSystem fileSystem)
		{
			author = doc.DocumentElement.GetAttribute("author");
			
			XmlElement config = doc.DocumentElement["Config"];
			name         = config.GetAttribute("name");
			icon         = SD.ResourceService.GetImage(config.GetAttribute("icon"));
			category     = config.GetAttribute("category");
			defaultName  = config.GetAttribute("defaultname");
			languagename = config.GetAttribute("language");
			
			if (config.HasAttribute("subcategory")) {
				subcategory = config.GetAttribute("subcategory");
			}

			string newFileDialogVisibleAttr  = config.GetAttribute("newfiledialogvisible");
			if (newFileDialogVisibleAttr != null && newFileDialogVisibleAttr.Length != 0) {
				if (newFileDialogVisibleAttr.Equals("false", StringComparison.OrdinalIgnoreCase))
					newFileDialogVisible = false;
			}

			if (doc.DocumentElement["Description"] != null) {
				description  = doc.DocumentElement["Description"].InnerText;
			}
			
			if (config["Wizard"] != null) {
				wizardpath = config["Wizard"].Attributes["path"].InnerText;
			}
			
			if (doc.DocumentElement["Properties"] != null) {
				XmlNodeList propertyList = doc.DocumentElement["Properties"].SelectNodes("Property");
				foreach (XmlElement propertyElement in propertyList) {
					properties.Add(new TemplateProperty(propertyElement));
				}
			}
			
			if (doc.DocumentElement["Types"] != null) {
				XmlNodeList typeList = doc.DocumentElement["Types"].SelectNodes("Type");
				foreach (XmlElement typeElement in typeList) {
					customTypes.Add(new TemplateType(typeElement));
				}
			}
			
			if (doc.DocumentElement["References"] != null) {
				XmlNodeList references = doc.DocumentElement["References"].SelectNodes("Reference");
				foreach (XmlElement reference in references) {
					if (!reference.HasAttribute("include"))
						throw new InvalidDataException("Reference without 'include' attribute!");
					ReferenceProjectItem item = new ReferenceProjectItem(null, reference.GetAttribute("include"));
					item.SetMetadata("HintPath", reference.GetAttribute("hintPath"));
					var requiredTargetFramework = reference.GetElementsByTagName("RequiredTargetFramework").OfType<XmlElement>().FirstOrDefault();
					if (requiredTargetFramework != null) {
						item.SetMetadata("RequiredTargetFramework", requiredTargetFramework.Value);
					}
					requiredAssemblyReferences.Add(item);
				}
			}
			
			if (doc.DocumentElement["Actions"] != null) {
				foreach (XmlElement el in doc.DocumentElement["Actions"]) {
					Action<FileTemplateResult> action = ReadAction(el);
					if (action != null)
						actions += action;
				}
			}
			
			fileoptions = doc.DocumentElement["AdditionalOptions"];
			
			// load the files
			XmlElement files  = doc.DocumentElement["Files"];
			XmlNodeList nodes = files.ChildNodes;
			foreach (XmlNode filenode in nodes) {
				if (filenode is XmlElement) {
					this.files.Add(new FileDescriptionTemplate((XmlElement)filenode, fileSystem));
				}
			}
		}
		
		static Action<FileTemplateResult> ReadAction(XmlElement el)
		{
			switch (el.Name) {
				case "RunCommand":
					if (el.HasAttribute("path")) {
						try {
							ICommand command = (ICommand)SD.AddInTree.BuildItem(el.GetAttribute("path"), null);
							return command.Execute;
						} catch (TreePathNotFoundException ex) {
							MessageService.ShowWarning(ex.Message + " - in " + el.OwnerDocument.DocumentElement.GetAttribute("fileName"));
							return null;
						}
					} else {
						ProjectTemplateImpl.WarnAttributeMissing(el, "path");
						return null;
					}
				default:
					ProjectTemplateImpl.WarnObsoleteNode(el, "Unknown action element is ignored");
					return null;
			}
		}
		
		public override void RunActions(FileTemplateResult result)
		{
			if (actions != null)
				actions(result);
		}
		
		public override string SuggestFileName(DirectoryName basePath)
		{
			if (defaultName.IndexOf("${Number}") >= 0) {
				try {
					int curNumber = 1;
					
					while (true) {
						string fileName = StringParser.Parse(defaultName, new StringTagPair("Number", curNumber.ToString()));
						if (basePath == null) {
							bool found = false;
							foreach (string openFile in FileService.GetOpenFiles()) {
								if (Path.GetFileName(openFile) == fileName) {
									found = true;
									break;
								}
							}
							if (found == false)
								return fileName;
						} else if (!File.Exists(Path.Combine(basePath, fileName))) {
							return fileName;
						}
						++curNumber;
					}
				} catch (Exception e) {
					MessageService.ShowException(e);
				}
			}
			return StringParser.Parse(defaultName);
		}
		
		public override object CreateCustomizationObject()
		{
			if (!HasProperties)
				return null;
			LocalizedTypeDescriptor localizedTypeDescriptor = new LocalizedTypeDescriptor();
			foreach (TemplateProperty property in Properties) {
				LocalizedProperty localizedProperty;
				if (property.Type.StartsWith("Types:")) {
					localizedProperty = new LocalizedProperty(property.Name, "System.Enum", property.Category, property.Description);
					TemplateType type = null;
					foreach (TemplateType templateType in CustomTypes) {
						if (templateType.Name == property.Type.Substring("Types:".Length)) {
							type = templateType;
							break;
						}
					}
					if (type == null) {
						throw new Exception("type : " + property.Type + " not found.");
					}
					localizedProperty.TypeConverterObject = new CustomTypeConverter(type);
					StringParserPropertyContainer.LocalizedProperty["Properties." + localizedProperty.Name] = property.DefaultValue;
					localizedProperty.DefaultValue = property.DefaultValue; // localizedProperty.TypeConverterObject.ConvertFrom();
				} else {
					localizedProperty = new LocalizedProperty(property.Name, property.Type, property.Category, property.Description);
					if (property.Type == "System.Boolean") {
						localizedProperty.TypeConverterObject = new BooleanTypeConverter();
						string defVal = property.DefaultValue == null ? null : property.DefaultValue.ToString();
						if (defVal == null || defVal.Length == 0) {
							defVal = "True";
						}
						StringParserPropertyContainer.LocalizedProperty["Properties." + localizedProperty.Name] = defVal;
						localizedProperty.DefaultValue = Boolean.Parse(defVal);
					} else {
						string defVal = property.DefaultValue == null ? String.Empty : property.DefaultValue.ToString();
						StringParserPropertyContainer.LocalizedProperty["Properties." + localizedProperty.Name] = defVal;
						localizedProperty.DefaultValue = defVal;
					}
				}
				localizedProperty.LocalizedName = property.LocalizedName;
				localizedTypeDescriptor.Properties.Add(localizedProperty);
			}
			return localizedTypeDescriptor;
		}
		
		public override FileTemplateResult Create(FileTemplateOptions options)
		{
			FileTemplateResult result = new FileTemplateResult(options);
			
			StandardHeader.SetHeaders();
			StringParserPropertyContainer.FileCreation["StandardNamespace"] = options.Namespace;
			StringParserPropertyContainer.FileCreation["FullName"]                 = options.FileName;
			StringParserPropertyContainer.FileCreation["FileName"]                 = Path.GetFileName(options.FileName);
			StringParserPropertyContainer.FileCreation["FileNameWithoutExtension"] = Path.GetFileNameWithoutExtension(options.FileName);
			StringParserPropertyContainer.FileCreation["Extension"]                = Path.GetExtension(options.FileName);
			StringParserPropertyContainer.FileCreation["Path"]                     = Path.GetDirectoryName(options.FileName);
			
			StringParserPropertyContainer.FileCreation["ClassName"] = options.ClassName;
			
			// when adding a file to a project (but not when creating a standalone file while a project is open):
			var project = options.Project;
			if (project != null && !options.IsUntitled) {
				// add required assembly references to the project
				foreach (ReferenceProjectItem reference in RequiredAssemblyReferences) {
					IEnumerable<ProjectItem> refs = project.GetItemsOfType(ItemType.Reference);
					if (!refs.Any(projItem => string.Equals(projItem.Include, reference.Include, StringComparison.OrdinalIgnoreCase))) {
						ReferenceProjectItem projItem = (ReferenceProjectItem)reference.CloneFor(project);
						ProjectService.AddProjectItem(project, projItem);
						ProjectBrowserPad.RefreshViewAsync();
					}
				}
			}
			
			foreach (FileDescriptionTemplate newfile in FileDescriptionTemplates) {
				if (!IsFilenameAvailable(StringParser.Parse(newfile.Name))) {
					MessageService.ShowError(string.Format("Filename {0} is in use.\nChoose another one", StringParser.Parse(newfile.Name))); // TODO : translate
					return null;
				}
			}
			var createdFiles = new List<KeyValuePair<string, FileDescriptionTemplate>>();
			ScriptRunner scriptRunner = new ScriptRunner();
			foreach (FileDescriptionTemplate newFile in FileDescriptionTemplates) {
				FileOperationResult opresult = FileUtility.ObservedSave(
					() => {
						string resultFile;
						if (!String.IsNullOrEmpty(newFile.BinaryFileName)) {
							resultFile = SaveFile(newFile, null, newFile.BinaryFileName);
						} else {
							resultFile = SaveFile(newFile, scriptRunner.CompileScript(this, newFile), null);
						}
						if (resultFile != null) {
							result.NewFiles.Add(FileName.Create(resultFile));
							createdFiles.Add(new KeyValuePair<string, FileDescriptionTemplate>(resultFile, newFile));
						}
					}, FileName.Create(StringParser.Parse(newFile.Name))
				);
				if (opresult != FileOperationResult.OK)
					return null;
			}
			
			if (project != null) {
				foreach (KeyValuePair<string, FileDescriptionTemplate> createdFile in createdFiles) {
					FileName fileName = FileName.Create(createdFile.Key);
					ItemType type = project.GetDefaultItemType(fileName);
					FileProjectItem newItem = new FileProjectItem(project, type);
					newItem.FileName = fileName;
					createdFile.Value.SetProjectItemProperties(newItem);
					project.Items.Add(newItem);
				}
				project.Save();
			}
			
			// raise FileCreated event for the new files.
			foreach (var fileName in result.NewFiles) {
				FileService.FireFileCreated(fileName, false);
			}
			return result;
		}
		
		bool IsFilenameAvailable(string fileName)
		{
			if (Path.IsPathRooted(fileName)) {
				return !File.Exists(fileName);
			}
			return true;
		}
		
		string SaveFile(FileDescriptionTemplate newfile, string content, string binaryFileName)
		{
			string unresolvedFileName = StringParser.Parse(newfile.Name);
			// Parse twice so that tags used in included standard header are parsed
			string parsedContent = StringParser.Parse(StringParser.Parse(content));
			
			if (parsedContent != null) {
				if (SD.EditorControlService.GlobalOptions.IndentationString != "\t") {
					parsedContent = parsedContent.Replace("\t", SD.EditorControlService.GlobalOptions.IndentationString);
				}
			}
			
			
			// when newFile.Name is "${Path}/${FileName}", there might be a useless '/' in front of the file name
			// if the file is created when no project is opened. So we remove single '/' or '\', but not double
			// '\\' (project is saved on network share).
			if (unresolvedFileName.StartsWith("/") && !unresolvedFileName.StartsWith("//")
			    || unresolvedFileName.StartsWith("\\") && !unresolvedFileName.StartsWith("\\\\"))
			{
				unresolvedFileName = unresolvedFileName.Substring(1);
			}
			
			if (newfile.IsDependentFile && Path.IsPathRooted(unresolvedFileName)) {
				Directory.CreateDirectory(Path.GetDirectoryName(unresolvedFileName));
				if (!String.IsNullOrEmpty(binaryFileName))
					File.Copy(binaryFileName, unresolvedFileName);
				else
					File.WriteAllText(unresolvedFileName, parsedContent, SD.FileService.DefaultFileEncoding);
			} else {
				if (!String.IsNullOrEmpty(binaryFileName)) {
					LoggingService.Warn("binary file was skipped");
					return null;
				}
				IViewContent viewContent = FileService.NewFile(Path.GetFileName(unresolvedFileName), parsedContent);
				if (viewContent == null) {
					return null;
				}
				if (Path.IsPathRooted(unresolvedFileName)) {
					Directory.CreateDirectory(Path.GetDirectoryName(unresolvedFileName));
					viewContent.PrimaryFile.SaveToDisk(FileName.Create(unresolvedFileName));
				}
			}
			return unresolvedFileName;
		}
		
	}
}
