﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using ICSharpCode.Core;
using ICSharpCode.SharpDevelop.Project;

namespace ICSharpCode.SharpDevelop.Templates
{
	public abstract class ProjectTemplate : TemplateBase
	{
		/// <summary>
		/// Gets whether this template is available within the specified solution.
		/// </summary>
		/// <param name="solution">The solution to which the new project should be added.
		/// Can be <c>null</c> when creating a new solution.</param>
		public virtual bool IsVisible(ISolution solution)
		{
			return true;
		}
		
		public virtual IEnumerable<TargetFramework> SupportedTargetFrameworks {
			get { return Enumerable.Empty<TargetFramework>(); }
		}
		
		public abstract ProjectTemplateResult CreateProjects(ProjectTemplateOptions options);
		
		internal ProjectTemplateResult CreateAndOpenSolution(ProjectTemplateOptions options, string solutionDirectory, string solutionName)
		{
			FileName solutionFileName = FileName.Create(Path.Combine(solutionDirectory, solutionName + ".sln"));
			bool solutionOpened = false;
			ISolution createdSolution = SD.ProjectService.CreateEmptySolutionFile(solutionFileName);
			try {
				options.Solution = createdSolution;
				options.SolutionFolder = createdSolution;
				var result = CreateProjects(options);
				if (result == null) {
					return null;
				}
				createdSolution.Save(); // solution must be saved before it can be opened
				if (SD.ProjectService.OpenSolution(createdSolution)) {
					solutionOpened = true;
					SD.GetRequiredService<IProjectServiceRaiseEvents>().RaiseSolutionCreated(new SolutionEventArgs(createdSolution));
					return result;
				} else {
					return null;
				}
			} finally {
				if (!solutionOpened)
					createdSolution.Dispose();
			}
		}
		
		/// <summary>
		/// Runs the actions after the newly created solution is opened in the IDE.
		/// </summary>
		public virtual void RunOpenActions(ProjectTemplateResult result)
		{
		}
	}
}
