using System;

namespace core
{
	public interface IPlatformMethods
	{
		void SetNoBackupFlag (string path);
		void SetNoBackupFlag (string path, int version);
	}
}
