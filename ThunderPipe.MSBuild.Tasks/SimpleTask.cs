using System;
using Task = Microsoft.Build.Utilities.Task;

namespace ThunderPipe.MSBuild.Tasks;

public class SimpleTask : Task
{
	/// <inheritdoc />
	public override bool Execute()
	{
		Console.WriteLine("TEST UWU owo");
		return true;
	}
}
