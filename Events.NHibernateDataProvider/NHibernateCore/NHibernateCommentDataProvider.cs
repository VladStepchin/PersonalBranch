﻿using System.Collections.Generic;
using NHibernate;
using NHibernate.Criterion;
using Events.Business.Models;
using Events.Business.Interfaces;

namespace Events.NHibernateDataProvider.NHibernateCore
{
    public class NHibernateCommentDataProvider : ICommentDataProvider
    {
        public IList<Comment> GetAll()
        {
            using (ISession session = Helper.OpenSession())
            {
                var criteria = session.CreateCriteria<Comment>();
                return criteria.List<Comment>();
            }
        }

        public Comment GetById(string id)
        {
            Comment Model;
            using (ISession session = Helper.OpenSession())
            {
                Model = session.Get<Comment>(id);
            }
            return Model;
        }
        public IList<Comment> GetByEventId(string eventId)
        {
            using (ISession session = Helper.OpenSession())
            {
                var criteria = session.CreateCriteria<Comment>();
                criteria.Add(Expression.Eq("eventId", eventId));
                return criteria.List<Comment>();
            }
        }

        public int Create(Comment model)
        {
            int EmpNo = 0;

            using (ISession session = Helper.OpenSession())
            {
                //Perform transaction
                using (ITransaction tran = session.BeginTransaction())
                {
                    session.Save(model);
                    tran.Commit();
                }
            }
            return EmpNo;
        }

        public void Update(Comment model)
        {
            using (ISession session = Helper.OpenSession())
            {
                using (ITransaction tran = session.BeginTransaction())
                {
                    session.Update(model);
                    tran.Commit();
                }
            }
        }

        public void Delete(Comment model)
        {
            using (ISession session = Helper.OpenSession())
            {
                using (ITransaction tran = session.BeginTransaction())
                {
                    session.Delete(model);
                    tran.Commit();
                }
            }
        }
    }
}