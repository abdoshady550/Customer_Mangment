using Customer_Mangment.Data;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.Model.Results;
using Customer_Mangment.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Customer_Mangment.Repository.Services.Background
{
    public sealed class DataMigrationService(
        AppDbContext sqlContext,
        IMongoDatabase mongoDatabase,
        ILogger<DataMigrationService> logger) : IMigrationService
    {
        private readonly IMongoCollection<Customer> _mongoCustomers =
            mongoDatabase.GetCollection<Customer>("Customers");

        private readonly IMongoCollection<Address> _mongoAddresses =
            mongoDatabase.GetCollection<Address>("Addresses");

        private readonly IMongoCollection<RefreshToken> _mongoTokens =
            mongoDatabase.GetCollection<RefreshToken>("RefreshTokens");

        private readonly IMongoCollection<User> _mongoUsers =
            mongoDatabase.GetCollection<User>("Users");

        //  SQL To  MongoDB
        public async Task<MigrationResult> MigrateSqlToMongoAsync(CancellationToken ct = default)
        {
            var errors = new List<string>();
            int customers = 0, addresses = 0, tokens = 0, users = 0;

            try
            {
                // Users 
                var sqlUsers = await sqlContext.Users
                    .AsNoTracking().ToListAsync(ct);

                foreach (var user in sqlUsers)
                {
                    try
                    {
                        await _mongoUsers.ReplaceOneAsync(
                            Builders<User>.Filter.Eq(u => u.Id, user.Id), user,
                            new ReplaceOptions { IsUpsert = true }, ct);
                        users++;
                    }
                    catch (Exception ex) { Capture(errors, "User", user.Id, ex); }
                }
                //Customers
                var sqlCustomers = await sqlContext.Customers
                    .IgnoreQueryFilters()
                    .Include(c => c.Addresses)
                    .AsNoTracking()
                    .ToListAsync(ct);

                foreach (var customer in sqlCustomers)
                {
                    try
                    {
                        await _mongoCustomers.ReplaceOneAsync(
                            Builders<Customer>.Filter.Eq(c => c.Id, customer.Id), customer,
                            new ReplaceOptions { IsUpsert = true }, ct);
                        customers++;

                        foreach (var address in customer.Addresses)
                        {
                            await _mongoAddresses.ReplaceOneAsync(
                                Builders<Address>.Filter.Eq(a => a.Id, address.Id), address,
                                new ReplaceOptions { IsUpsert = true }, ct);
                            addresses++;
                        }
                    }
                    catch (Exception ex) { Capture(errors, "Customer", customer.Id.ToString(), ex); }
                }
                //RefreshTokens
                var sqlRefreshTokens = await sqlContext.RefreshTokens
                    .AsNoTracking().ToListAsync(ct);

                foreach (var token in sqlRefreshTokens)
                {
                    try
                    {
                        await _mongoTokens.ReplaceOneAsync(
                            Builders<RefreshToken>.Filter.Eq(r => r.Id, token.Id), token,
                            new ReplaceOptions { IsUpsert = true }, ct);
                        tokens++;
                    }
                    catch (Exception ex) { Capture(errors, "RefreshToken", token.Id.ToString(), ex); }
                }

                logger.LogInformation(
                    "SQL→Mongo done. Users={U} Customers={C} Addresses={A} Tokens={T} Errors={E}",
                    users, customers, addresses, tokens, errors.Count);

                return errors.Count == 0
                    ? MigrationResult.Ok("SQL → MongoDB", customers, addresses, tokens, users)
                    : MigrationResult.Fail("SQL → MongoDB", errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "SQL→Mongo: fatal error");
                return MigrationResult.Fail("SQL → MongoDB", [$"Fatal: {ex.Message}"]);
            }
        }

        //  MongoDB To  SQL
        public async Task<MigrationResult> MigrateMongoToSqlAsync(CancellationToken ct = default)
        {
            var errors = new List<string>();
            int customers = 0, addresses = 0, tokens = 0, users = 0;

            try
            {
                // Users 
                var mongoUsers = await _mongoUsers.Find(_ => true).ToListAsync(ct);

                var existingUserIds = (await sqlContext.Users
                    .Select(u => u.Id).ToListAsync(ct)).ToHashSet();

                foreach (var user in mongoUsers)
                {
                    try
                    {
                        if (existingUserIds.Contains(user.Id))
                        {
                            DetachIfTracked<User>(user.Id);
                            sqlContext.Users.Update(user);
                        }
                        else
                        {
                            sqlContext.Users.Add(user);
                            existingUserIds.Add(user.Id);
                        }
                        users++;
                    }
                    catch (Exception ex) { Capture(errors, "User", user.Id, ex); }
                }

                await sqlContext.SaveChangesAsync(ct);
                sqlContext.ChangeTracker.Clear();

                //Customers
                var mongoCustomers = await _mongoCustomers
                    .Find(Builders<Customer>.Filter.Empty).ToListAsync(ct);

                var existingCustomerIds = (await sqlContext.Customers
                    .IgnoreQueryFilters()
                    .Select(c => c.Id).ToListAsync(ct)).ToHashSet();

                foreach (var customer in mongoCustomers)
                {
                    try
                    {
                        ClearAddressList(customer);

                        if (existingCustomerIds.Contains(customer.Id))
                        {
                            DetachIfTracked<Customer>(customer.Id.ToString());
                            sqlContext.Customers.Update(customer);
                        }
                        else
                        {
                            sqlContext.Customers.Add(customer);
                            existingCustomerIds.Add(customer.Id);
                        }
                        customers++;
                    }
                    catch (Exception ex) { Capture(errors, "Customer", customer.Id.ToString(), ex); }
                }

                await sqlContext.SaveChangesAsync(ct);
                sqlContext.ChangeTracker.Clear();

                // Addresses 
                var mongoAddresses = await _mongoAddresses
                    .Find(Builders<Address>.Filter.Empty).ToListAsync(ct);

                var existingAddressIds = (await sqlContext.Set<Address>()
                    .IgnoreQueryFilters()
                    .Select(a => a.Id).ToListAsync(ct)).ToHashSet();

                foreach (var address in mongoAddresses)
                {
                    try
                    {
                        if (!existingCustomerIds.Contains(address.CustomerId))
                        {
                            errors.Add($"Address {address.Id}: parent customer {address.CustomerId} not found in SQL.");
                            continue;
                        }

                        NullCustomerNavigation(address);

                        if (existingAddressIds.Contains(address.Id))
                        {
                            sqlContext.Set<Address>().Update(address);
                        }
                        else
                        {
                            sqlContext.Set<Address>().Add(address);
                            existingAddressIds.Add(address.Id);
                        }
                        addresses++;
                    }
                    catch (Exception ex) { Capture(errors, "Address", address.Id.ToString(), ex); }
                }

                await sqlContext.SaveChangesAsync(ct);
                sqlContext.ChangeTracker.Clear();

                // Refresh Tokens
                var mongoRefreshTokens = await _mongoTokens
                    .Find(Builders<RefreshToken>.Filter.Empty).ToListAsync(ct);

                var existingTokenIds = (await sqlContext.RefreshTokens
                    .Select(r => r.Id).ToListAsync(ct)).ToHashSet();

                foreach (var token in mongoRefreshTokens)
                {
                    try
                    {
                        if (existingTokenIds.Contains(token.Id))
                            sqlContext.RefreshTokens.Update(token);
                        else
                            sqlContext.RefreshTokens.Add(token);

                        tokens++;
                    }
                    catch (Exception ex) { Capture(errors, "RefreshToken", token.Id.ToString(), ex); }
                }

                await sqlContext.SaveChangesAsync(ct);
                sqlContext.ChangeTracker.Clear();

                logger.LogInformation(
                    "Mongo→SQL done. Users={U} Customers={C} Addresses={A} Tokens={T} Errors={E}",
                    users, customers, addresses, tokens, errors.Count);

                return errors.Count == 0
                    ? MigrationResult.Ok("MongoDB → SQL", customers, addresses, tokens, users)
                    : MigrationResult.Fail("MongoDB → SQL", errors);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MongoTo SQL: fatal error");
                return MigrationResult.Fail("MongoDB → SQL", [$"Fatal: {ex.Message}"]);
            }
        }


        private void DetachIfTracked<T>(object keyValue) where T : class
        {
            var entry = sqlContext.ChangeTracker
                .Entries<T>()
                .FirstOrDefault(e =>
                    e.Property("Id").CurrentValue?.ToString() == keyValue.ToString());

            if (entry is not null)
                entry.State = EntityState.Detached;
        }

        private static void ClearAddressList(Customer customer)
        {
            var field = typeof(Customer).GetField(
                "_addresses",
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            (field?.GetValue(customer) as List<Address>)?.Clear();
        }
        private static void NullCustomerNavigation(Address address)
        {
            var prop = typeof(Address).GetProperty(
                "Customer",
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance);

            prop?.SetValue(address, null);
        }

        private void Capture(List<string> errors, string entity, object id, Exception ex)
        {
            var msg = $"{entity} {id}: {ex.Message}";
            logger.LogError(ex, "Migration error: {Msg}", msg);
            errors.Add(msg);
        }
    }
}
