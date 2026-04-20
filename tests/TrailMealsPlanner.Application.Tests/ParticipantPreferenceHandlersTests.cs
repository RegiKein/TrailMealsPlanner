using TrailMealsPlanner.Application.Interfaces;
using TrailMealsPlanner.Application.UseCases;
using TrailMealsPlanner.Domain.Entities;
using TrailMealsPlanner.Domain.Enums;

namespace TrailMealsPlanner.Application.Tests;

public sealed class ParticipantPreferenceHandlersTests
{
    [Fact]
    public async Task AddParticipant_And_SetProductPreference_WorkEndToEnd()
    {
        var participantRepository = new TestParticipantRepository();
        var preferenceRepository = new TestProductPreferenceRepository();
        var productRepository = new TestProductRepository(new Product("Rice", 350, 7, 1, 78));

        var addParticipantHandler = new AddParticipantHandler(participantRepository);
        var setPreferenceHandler = new SetProductPreferenceHandler(preferenceRepository);
        var getParticipantsHandler = new GetParticipantsHandler(participantRepository);
        var getPreferencesHandler = new GetProductPreferencesHandler(participantRepository, preferenceRepository, productRepository);

        var participantId = await addParticipantHandler.Handle(new AddParticipantCommand { Name = "Anna" });
        var productId = (await productRepository.GetAllAsync()).Single().Id;

        await setPreferenceHandler.Handle(new SetProductPreferenceCommand
        {
            ParticipantId = participantId,
            ProductId = productId,
            PreferenceLevel = PreferenceLevel.Allergy,
            Comment = "Нельзя арахис и следы"
        });

        var participants = await getParticipantsHandler.Handle(new GetParticipantsQuery());
        var preferences = await getPreferencesHandler.Handle(new GetProductPreferencesQuery());

        Assert.Single(participants);
        Assert.Single(preferences);
        Assert.Equal("Anna", participants[0].Name);
        Assert.Equal(PreferenceLevel.Allergy, preferences[0].PreferenceLevel);
    }

    private sealed class TestParticipantRepository : IParticipantRepository
    {
        private readonly List<Participant> participants = [];

        public Task AddAsync(Participant participant, CancellationToken cancellationToken = default)
        {
            participants.Add(participant);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Participant>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Participant>>(participants);
        }
    }

    private sealed class TestProductPreferenceRepository : IProductPreferenceRepository
    {
        private readonly List<ProductPreference> preferences = [];

        public Task UpsertAsync(ProductPreference preference, CancellationToken cancellationToken = default)
        {
            var existing = preferences.FirstOrDefault(item => item.ParticipantId == preference.ParticipantId && item.ProductId == preference.ProductId);
            if (existing is null)
            {
                preferences.Add(preference);
            }
            else
            {
                existing.Update(preference.PreferenceLevel, preference.Comment);
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<ProductPreference>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<ProductPreference>>(preferences);
        }
    }

    private sealed class TestProductRepository : IProductRepository
    {
        private readonly List<Product> products;

        public TestProductRepository(params Product[] products)
        {
            this.products = products.ToList();
        }

        public Task AddAsync(Product product, CancellationToken cancellationToken = default)
        {
            products.Add(product);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Product>>(products);
        }

        public Task<IReadOnlyList<Product>> GetByIdsAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyList<Product>>(products.Where(product => productIds.Contains(product.Id)).ToList());
        }
    }
}
