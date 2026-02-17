using GroupAllocator.Controllers;
using GroupAllocator.Services;

namespace GroupAllocator;

public static class ServicesRegistration
{
	public static void RegisterApplicationServices(this IServiceCollection collection)
	{
		collection
			.AddScoped<IUserService, UserService>()
			.AddScoped<IAllocationSolver, AllocationSolver>()
			.AddScoped<PaymentService>()
			.AddScoped<ProjectsService>()
			.AddScoped<StudentsService>()
			;
	}
}
