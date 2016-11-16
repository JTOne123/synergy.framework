﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using NHibernate;
using Synergy.Contracts;
using Synergy.NHibernate.Engine;
using Synergy.Reflection;

namespace Synergy.NHibernate.Transactions
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class TransactionCoordinator : ITransactionCoordinator
    {
        private readonly IDatabaseProvider databaseProvider;

        public TransactionCoordinator(IDatabaseProvider databaseProvider)
        {
            this.databaseProvider = databaseProvider;
        }

        public TransactionsContainer StartTransactionsFor(MethodInfo method)
        {
            Fail.IfArgumentNull(method, nameof(method));

            List<AutoTransactionAttribute> transactionAttributes = TransactionCoordinator.GetAutoTransactionAttributesFor(method);

            // Remove all the attributes with disabled transaction
            var enabledTransactions =  transactionAttributes.Where(t => t.Disabled == false).ToArray();
            var disabledTransactions = transactionAttributes.Where(t => t.Disabled).ToArray();

            this.FailIfTransactionStartedDespiteDisablingIt(disabledTransactions);

            return this.AutostartTransactions(enabledTransactions);
        }

        private void FailIfTransactionStartedDespiteDisablingIt([NotNull] AutoTransactionAttribute[] disabledTransactions)
        {
            Fail.IfArgumentNull(disabledTransactions, nameof(disabledTransactions));

            foreach (AutoTransactionAttribute disabledTransaction in disabledTransactions)
            {
                IDatabase database = this.GetDatabaseForAutoTransaction(disabledTransaction);
                ISession session = database.CurrentSession;
                Fail.IfTrue(session.Transaction.IsActive,
                    "Transaction is started to database {0} and it shouldn't be due to attribute {1}",
                    database,
                    disabledTransaction);
            }
        }

        [NotNull]
        private TransactionsContainer AutostartTransactions([NotNull] AutoTransactionAttribute[] enabledTransactions)
        {
            Fail.IfArgumentNull(enabledTransactions, nameof(enabledTransactions));

            var container = new TransactionsContainer();
            foreach (AutoTransactionAttribute transactionalAttribute in enabledTransactions)
            {
                IDatabase database = this.GetDatabaseForAutoTransaction(transactionalAttribute);
                ISession session = database.CurrentSession;
                ITransaction transaction = session.BeginTransaction(transactionalAttribute.IsolationLevel);
                container.Add(database, session, transaction);
            }

            return container;
        }

        [NotNull] 
        private IDatabase GetDatabaseForAutoTransaction([NotNull] AutoTransactionAttribute transactionalAttribute)
        {
            Fail.IfArgumentNull(transactionalAttribute, nameof(transactionalAttribute));

            Type databaseType = transactionalAttribute.On;
            Fail.IfNull(databaseType, "There is no database pointed in {0}", transactionalAttribute);

            IDatabase database = this.databaseProvider.Get(databaseType);
            Fail.IfNull(database,
                "Could not find database used in {0}. Did you point {1} in the '{2}' argument attribute?",
                transactionalAttribute,
                nameof(IDatabase),
                nameof(AutoTransactionAttribute.On));

            return database;
        }

        [NotNull]
        private static List<AutoTransactionAttribute> GetAutoTransactionAttributesFor([NotNull] MethodInfo method)
        {
            Fail.IfArgumentNull(method, nameof(method));

            AutoTransactionAttribute[] transactionsOnMethod = method.GetCustomAttributesBasedOn<AutoTransactionAttribute>();
            AutoTransactionAttribute[] transactionsOnClass = method.DeclaringType.OrFail(nameof(MemberInfo.DeclaringType))
                                                                   .GetCustomAttributesBasedOn<AutoTransactionAttribute>();

            List<AutoTransactionAttribute> transactionAttributes = transactionsOnMethod.ToList();
            foreach (AutoTransactionAttribute transactionalAttribute in transactionsOnClass)
                if (transactionAttributes.Any(attr => attr.On == transactionalAttribute.On) == false)
                    transactionAttributes.Add(transactionalAttribute);

            return transactionAttributes;
        }
    }

    public interface ITransactionCoordinator
    {
        [NotNull]
        TransactionsContainer StartTransactionsFor([NotNull] MethodInfo method);
    }
}