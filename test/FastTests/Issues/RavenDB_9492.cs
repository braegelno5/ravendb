﻿using System.Collections.Generic;
using System.Linq;
using Raven.Client.Documents;
using Xunit;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Operations.Indexes;

namespace FastTests.Issues
{
    public class RavenDB_9492: RavenTestBase
    {
        [Fact]
        public void CanQueryIdWithNegate()
        {
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new Bunny{Name="what"}, "bunny/1");
                    session.Store(new Bunny { Name = "is" }, "bunny/2");
                    session.Store(new Bunny { Name = "your" }, "bunny/3");
                    session.SaveChanges();
                    var query = session.Query<Bunny>().Where(u => u.Id != "bunny/1").ToList();
                    //We want to make sure this kind of query generates an index
                    var indexNames = store.Maintenance.Send(new GetIndexNamesOperation(0, 10));
                    Assert.Equal(1, indexNames.Length);
                }
            }
        }

        [Fact]
        public void InQueryOnIdsShouldRunOnCollection()
        {
            var bunnies = new List<string>
            {
                "bunny/1",
                "bunny/2"
            };
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new Bunny { Name = "what" }, "bunny/1");
                    session.Store(new Bunny { Name = "is" }, "bunny/2");
                    session.Store(new Bunny { Name = "your" }, "bunny/3");
                    session.SaveChanges();
                    var query = session.Query<Bunny>().Where(u => u.Id.In(bunnies)).ToList();
                    Assert.Equal(2, query.Count);
                    //We want to make sure no index was created for such query
                    var indexNames = store.Maintenance.Send(new GetIndexNamesOperation(0, 10));
                    Assert.Empty(indexNames);
                }
            }
        }

        [Fact]
        public void NotInQueryOnIdsShouldntRunOnCollection()
        {
            var bunnies = new List<string>
            {
                "bunny/1",
                "bunny/2"
            };
            using (var store = GetDocumentStore())
            {
                using (var session = store.OpenSession())
                {
                    session.Store(new Bunny { Name = "what" }, "bunny/1");
                    session.Store(new Bunny { Name = "is" }, "bunny/2");
                    session.Store(new Bunny { Name = "your" }, "bunny/3");
                    session.SaveChanges();
                    var query = session.Query<Bunny>().Where(u => !u.Id.In(bunnies)).Customize(x=>x.WaitForNonStaleResults()).ToList();
                    WaitForUserToContinueTheTest(store);
                    Assert.Equal(1, query.Count);
                    //We want to make sure this kind of query generates an index
                    var indexNames = store.Maintenance.Send(new GetIndexNamesOperation(0, 10));
                    Assert.Equal(1, indexNames.Length);
                }
            }
        }
        private class Bunny
        {
            public string Id { get; set; }
            public string Name { get; set; }
        }
    }
}
