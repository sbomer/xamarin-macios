﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Framework;
using Xamarin.Messaging.Build.Client;

﻿namespace Xamarin.MacDev.Tasks
{
	public class CollectFrameworks : CollectFrameworksBase, ITaskCallback, ICancelableTask
	{
		public override bool Execute ()
		{
			if (!string.IsNullOrEmpty (SessionId))
				return new TaskRunner (SessionId, BuildEngine4).RunAsync (this).Result;

			return base.Execute ();
		}

		public IEnumerable<ITaskItem> GetAdditionalItemsToBeCopied () => Enumerable.Empty<ITaskItem> ();

		public bool ShouldCopyToBuildServer (ITaskItem item) => false;

		public bool ShouldCreateOutputFile (ITaskItem item) => false;

		public void Cancel ()
		{
			if (!string.IsNullOrEmpty (SessionId))
				BuildConnection.CancelAsync (SessionId, BuildEngine4).Wait ();
		}
	}
}

