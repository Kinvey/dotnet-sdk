using System;
using System.Threading.Tasks;
using System.Linq;
using Remotion.Linq;

namespace KinveyXamarin
{
	public abstract class ReadRequest <T, U> : Request <T, U>
	{
		public ICache<T> Cache { get; }
		public string Collection { get; }
		public ReadPolicy Policy { get; }
		protected IQueryable<T> Query { get; }

		public ReadRequest (AbstractClient client, string collection, ICache<T> cache, IQueryable<T> query, ReadPolicy policy)
			: base(client)
		{
			this.Cache = cache;
			this.Collection = collection;
			this.Query = query;
			this.Policy = policy;
		}


		protected string BuildMongoQuery ()
		{
			if (Query != null) {
				StringQueryBuilder Writer = new StringQueryBuilder ();
				Writer.Reset ();

				KinveyQueryVisitor visitor = new KinveyQueryVisitor (Writer, typeof (T));

				QueryModel queryModel = (Query.Provider as KinveyQueryProvider).qm;

				Writer.Write ("{");
				queryModel.Accept (visitor);
				Writer.Write ("}");

				string mongoQuery = Writer.GetFullString ();

				return mongoQuery;
			}

			return default (string);
		}

	}
}

