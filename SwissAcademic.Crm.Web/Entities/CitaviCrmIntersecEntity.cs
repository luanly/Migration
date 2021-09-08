using Microsoft.Azure.Documents.SystemFunctions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SwissAcademic.Crm.Web
{
	public class CitaviCrmIntersecEntity
		:
		IEquatable<CitaviCrmIntersecEntity>
	{
		public const string FETCHXML_ALIAS = "intersec";

		public Guid Entity1Id;
		public Guid Entity2Id;
		public Guid RelationshipId;

		public bool Equals([AllowNull] CitaviCrmIntersecEntity other)
		{
			if (other == null)
			{
				return false;
			}
			return Entity1Id == other.Entity1Id &&
				   Entity2Id == other.Entity2Id &&
				   RelationshipId == other.RelationshipId;
		}

		public static CitaviCrmIntersecEntity Parse(string odata)
		 => Parse(JObject.Parse(odata));

		public static CitaviCrmIntersecEntity Parse(JObject token)
		{
			var intersecEntity = new CitaviCrmIntersecEntity();

			foreach (var props in token)
			{
				if (!props.Key.StartsWith(FETCHXML_ALIAS))
				{
					continue;
				}

				var id = Guid.Parse(props.Value.Value<string>());
				if(intersecEntity.Entity1Id == Guid.Empty)
				{
					intersecEntity.Entity1Id = id;
				}
				else if (intersecEntity.RelationshipId == Guid.Empty)
				{
					intersecEntity.RelationshipId = id;
				}
				else if (intersecEntity.Entity2Id == Guid.Empty)
				{
					intersecEntity.Entity2Id = id;
					break;
				}
			}

			return intersecEntity;
		}

		public override string ToString()
		{
			return $"{Entity1Id}:{Entity2Id}";
		}

		
	}
}
