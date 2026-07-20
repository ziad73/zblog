- Frontend Consumes backend APIs, so we have to keep the API self-documenting with data(for redirect displying data), no bare message
  - Always use Response Dto 
- In a Web API setup, the frontend client owns the routing state and handles the returnUrl flow entirely.
- The backend simply acts as a stateless validator that enforces security rules.
- No need for Repository Pattern
- Clean Arch stuff
- Record(Positional Record style or standard Record Body (Recommended for API DTOs)) alt for class DTOs && add data annotations as you want!
  - init: private set, that's all DTOs need
- decorate your controller with [ApiController], ASP.NET Core automatically registers an invisible action filter called ModelStateInvalidFilter, No need to check ModelState.IsValid at every endpoint


- EF Core's Select projection is smart — when you access c.author.UserName in a Select, it generates the SQL JOIN automatically. Include is only needed when you're returning the entity itself and want its navigation properties populated (eager loading). Inside a Select, EF Core figures out the joins from the expression tree.
