using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace SwissAcademic.Crm.Web
{
    public class CrmSet
    {
        #region Konstruktor

        public CrmSet(IEnumerable<CitaviCrmEntity> collection)
        {
            FromDataCollection(collection);
        }

        #endregion

        #region Eigenschaften

        public IEnumerable<Account> Accounts { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
        public IEnumerable<CampusContract> CampusContracts { get; set; }
        public IEnumerable<CleverbridgeProduct> CleverbridgeProducts { get; set; }
        public IEnumerable<LinkedAccount> CrmLinkedAccounts { get; set; }
        public IEnumerable<LinkedEmailAccount> CrmLinkedEMailAccounts { get; set; }
        public IEnumerable<EmailDomain> EMailDomains { get; set; }
        public IEnumerable<CitaviLicense> Licenses { get; set; }
        public IEnumerable<LicenseType> LicenseTypes { get; set; }
        public IEnumerable<IPRange> IPRanges { get; set; }
        public IEnumerable<OrderProcess> OrderProcesses { get; set; }
        public IEnumerable<OrganizationSetting> OrganizationSettings { get; set; }
        public IEnumerable<Pricing> Pricings { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<ProjectEntry> ProjectEntries { get; set; }
        public IEnumerable<ProjectRole> ProjectRoles { get; set; }
        public IEnumerable<Voucher> Vouchers { get; set; }
        public IEnumerable<VoucherBlock> VoucherBlocks { get; set; }
        public IEnumerable<Subscription> Subscriptions{ get; set; }
        public IEnumerable<SubscriptionItem> SubscriptionItems { get; set; }

        #endregion

        #region Methoden

        #region FromDataCollection

        int FromDataCollection(IEnumerable<CitaviCrmEntity> collection)
        {
            var items = new Dictionary<string, CitaviCrmEntity>();
            if (collection == null)
            {
                return 0;
            }

            foreach (var item in collection)
            {
                var criteraAttributes = new List<KeyValuePair<string, object>>();
                foreach (var attribute in item.Attributes)
                {
                    if (attribute.Key.StartsWith(FetchXmlConstants.CriteriaPrefix))
                    {
                        criteraAttributes.Add(attribute);
                    }
                }
                criteraAttributes.ForEach(i => item.Attributes.Remove(i.Key));
            }

            foreach (var entity in collection)
            {
                var entityType = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == entity.LogicalName).Type;

                if (!items.ContainsKey(entity.Key))
                {
                    items.Add(entity.Key, entity);
                }
                var entityAttributes = entity.Attributes.Where(i => !(i.Value is AliasedValue)).ToList();
                var aliasedAttributes = entity.Attributes.Where(i => (i.Value is AliasedValue)).GroupBy(i => ((AliasedValue)i.Value).EntityLogicalName).ToList();
                var intersectAttributes = entity.Attributes.Where(i => (i.Value is AliasedValue)).Where(i => RelationshipResolver.IsIntersectType(((AliasedValue)i.Value).EntityLogicalName)).Select(i => (AliasedValue)i.Value).ToList();
                foreach (var intersectAttribute in intersectAttributes)
                {
                    items.TryGetValue(entity.Key, out CitaviCrmEntity item);
                    item.AddIntersectAttribute(intersectAttribute.EntityLogicalName, intersectAttribute.Value);
                }
                foreach (var aliasedAttribute in aliasedAttributes)
                {
                    if (RelationshipResolver.IsIntersectType(aliasedAttribute.Key))
                    {
                        continue;
                    }

                    var typeName = aliasedAttribute.Key;
                    var type = EntityNameResolver.KnownTypes.First(i => i.EntityLogicalName == typeName).Type;
                    var typeKeyPattern = string.Concat(".", typeName, ".new_key");
                    var aliasedItemKey = ((AliasedValue)aliasedAttribute.First(i => i.Key.EndsWith(typeKeyPattern)).Value).Value;

                    var entityKey = aliasedItemKey.ToString();
                    if (!items.TryGetValue(entityKey, out CitaviCrmEntity item))
                    {
                        item = Activator.CreateInstance(type) as CitaviCrmEntity;
                        items.Add(entityKey, item);
                    }
                    else
                    {
                        continue;
                    }

                    foreach (var attribute in aliasedAttributes.Where(i => i.Key == typeName).SelectMany(i => i))
                    {
                        var localAttribute = attribute.Value as AliasedValue;
                        item.SetValueWithChecks(localAttribute.AttributeLogicalName, localAttribute.Value);
                    }

                    var allOtherAliasedAttributes = aliasedAttributes.Where(i => i.Key != typeName).SelectMany(i => i).ToList();
                    foreach (var aliased in allOtherAliasedAttributes)
                    {
                        var val = aliased.Value as AliasedValue;
                        item.SetAliasedValue(aliased.Key, val);
                    }

                    var relationshipNames = RelationshipResolver.GetFirstRelationshipNames(entityType, type);
                    if (relationshipNames != null &&
                        relationshipNames.Count() == 1)
                    {
                        var name = EntityNameResolver.GetEntityAliasePrefix(relationshipNames.First());
                        if (entity.Attributes.Any(i => i.Key.StartsWith(name)))
                        {
                            //Hier besteht eine direkte Beziehung. Die Relation gibt es u. es gibt AliasedAttribute mit diesem Namen
                            foreach (var attr in entityAttributes)
                            {
                                item.SetAliasedValue(name, entity.LogicalName, attr.Key, attr.Value);
                            }
                        }
                    }
                }
            }


            int count = items.Count;
            var groupedItems = items.Values.GroupBy(i => i.Key).Select(i => i.First()).ToList();

            Accounts = groupedItems.Where(i => i is Account).Cast<Account>().ToList();
            Contacts = groupedItems.Where(i => i is Contact).Cast<Contact>().ToList();
            CrmLinkedAccounts = groupedItems.Where(i => i is LinkedAccount).Cast<LinkedAccount>().ToList();
            CrmLinkedEMailAccounts = groupedItems.Where(i => i is LinkedEmailAccount).Cast<LinkedEmailAccount>().ToList();
            CampusContracts = groupedItems.Where(i => i is CampusContract).Cast<CampusContract>().ToList();
            CleverbridgeProducts = groupedItems.Where(i => i is CleverbridgeProduct).Cast<CleverbridgeProduct>().ToList();
            EMailDomains = groupedItems.Where(i => i is EmailDomain).Cast<EmailDomain>().ToList();
            Licenses = groupedItems.Where(i => i is CitaviLicense).Cast<CitaviLicense>().ToList();
            LicenseTypes = groupedItems.Where(i => i is LicenseType).Cast<LicenseType>().ToList();
            IPRanges = groupedItems.Where(i => i is IPRange).Cast<IPRange>().ToList();
            OrderProcesses = groupedItems.Where(i => i is OrderProcess).Cast<OrderProcess>().ToList();
            OrganizationSettings = groupedItems.Where(i => i is OrganizationSetting).Cast<OrganizationSetting>().ToList();
            ProjectEntries = groupedItems.Where(i => i is ProjectEntry).Cast<ProjectEntry>().ToList();
            ProjectRoles = groupedItems.Where(i => i is ProjectRole).Cast<ProjectRole>().ToList();
            Pricings = groupedItems.Where(i => i is Pricing).Cast<Pricing>().ToList();
            Products = groupedItems.Where(i => i is Product).Cast<Product>().ToList();
            Vouchers = groupedItems.Where(i => i is Voucher).Cast<Voucher>().ToList();
            VoucherBlocks = groupedItems.Where(i => i is VoucherBlock).Cast<VoucherBlock>().ToList();
            Subscriptions = groupedItems.Where(i => i is Subscription).Cast<Subscription>().ToList();
            SubscriptionItems = groupedItems.Where(i => i is SubscriptionItem).Cast<SubscriptionItem>().ToList();

            return count;
        }

        #endregion

        #endregion
    }
}
