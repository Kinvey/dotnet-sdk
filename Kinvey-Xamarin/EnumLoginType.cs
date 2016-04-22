using System;

namespace KinveyXamarin
{
	/// <summary>
	/// the available login types
	/// </summary>
	public enum LoginType
	{
		/// <summary>
		/// Implicit login type
		/// </summary>
		IMPLICIT,
		/// <summary>
		/// Kinvey login type (username and password)
		/// </summary>
		KINVEY,
		/// <summary>
		/// Credential store login type
		/// </summary>
		CREDENTIALSTORE,
		/// <summary>
		/// Third party provider login type
		/// </summary>
		THIRDPARTY
	}
}

