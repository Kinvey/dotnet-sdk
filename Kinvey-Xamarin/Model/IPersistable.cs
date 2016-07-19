namespace KinveyXamarin
{
	/// <summary>
	/// Persistable interface which model objects can choose to implement as an alternative
	/// to subclassing from <see cref="Entity"/>
	/// </summary>
	public interface IPersistable
	{
		/// <summary>
		/// ID field which maps back to Kinvey _id
		/// </summary>
		/// <value>The identifier.</value>
		string ID { get; set; }

		/// <summary>
		/// <see cref="AccessControlList"/>  field which maps back to Kinvey _acl
		/// </summary>
		/// <value>The acl.</value>
		AccessControlList ACL { get; set; }

		/// <summary>
		/// <see cref="KinveyMetaData"/> field which maps back to Kinvey _kmd
		/// </summary>
		/// <value>The kmd.</value>
		KinveyMetaData KMD { get; set; }
	}
}
