using System.Diagnostics;
using System.Net;
using System.Text;
using Kassa.Models;
using Kassa.Services;
using Newtonsoft.Json;

public class ApiService
{
    private readonly HttpClient _httpClient;

    //Niet vergeten dit aan te passen naargelang het ipadres van computer
    private string ipAdres = "http://192.168.1.6";

    public ApiService()
    {
        _httpClient = new HttpClient(new HttpClientHandler());
    }

    //account en rechten

    public async Task<(bool IsSuccess, string? ErrorMessage)> RegisterUserAsync(string email, string password, string confirmPassword, string voornaam, string achternaam, string telefoonnummer)
    {
        var registerModel = new
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword,
            Voornaam = voornaam,
            Achternaam = achternaam,
            PhoneNumber = telefoonnummer
        };

        var json = JsonConvert.SerializeObject(registerModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ipAdres}:7153/api/account/register", content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ValidationErrorResponse>(errorContent);

            if (errorResponse != null && errorResponse.Errors?.Count > 0)
            {
                var translatedErrors = errorResponse.Errors
                    .SelectMany(err => err.Value)
                    .Select(errorMessage => TranslateErrorMessage(errorMessage))
                    .ToList();

                var allErrorMessage = string.Join(" ", translatedErrors); // Combineer alle vertaalde foutberichten in één string
                return (false, allErrorMessage);
            }

            return (false, "Er is een onbekende fout opgetreden.");
        }

        return (true, null);
    }

    public string TranslateErrorMessage(string errorMessage)
    {
        var translations = new Dictionary<string, string>
    {
        {"The Email field is required.", "Het veld Emailadres is verplicht."},
        {"The Password field is required.", "Het veld Wachtwoord is verplicht."},
        {"The Voornaam field is required.", "Het veld Voornaam is verplicht."},
        {"The Achternaam field is required.", "Het veld Achternaam is verplicht."},
        {"The ConfirmPassword field is required.", "Het veld Bevestig Wachtwoord is verplicht."},
        {"The password must be at least 6 and at max 100 characters long.", "Het wachtwoord moet minstens 6 tekens lang zijn."},
        {"The password and confirmation password do not match.", "Het wachtwoord en het herhaalde wachtwoord zijn niet gelijk."},
        {"The Email field is not a valid e-mail address.", "Het veld Emailadres is geen geldig emailadres."},
        // Voeg meer mappings toe zoals nodig
    };

        if (translations.TryGetValue(errorMessage, out var translatedMessage))
        {
            return translatedMessage;
        }

        return errorMessage; // Geen vertaling gevonden, retourneer origineel bericht
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var loginModel = new { Email = email, Password = password };
        var json = JsonConvert.SerializeObject(loginModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync($"{ipAdres}:7153/api/account/login", content);
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var loginResult = JsonConvert.DeserializeObject<LoginResult>(responseContent);

            // Save the token and userId to SecureStorage
            await SecureStorage.SetAsync("jwt_token", loginResult!.Token);
            await SecureStorage.SetAsync("user_id", loginResult.UserId);
            await SecureStorage.SetAsync("user_email", loginResult.Email);

            return loginResult.Token;
        }
        return null!;
    }

    //Verkrijg userid
    public async Task<Gebruiker> GetUserDetailsAsync(string email)
    {
        await SetBearerToken();

        try
        {
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/account/detailsByEmail?email={Uri.EscapeDataString(email)}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var user = JsonConvert.DeserializeObject<Gebruiker>(content);
                return user;
            }
            else
            {
                Debug.WriteLine($"Failed to retrieve user details: {response.StatusCode}");
                return null!;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetUserDetailsAsync: {ex.Message}");
            return null!;
        }
    }

    public class LoginResult
    {
        public string Token { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public string Email { get; set; } = default!;
        GebruikerRechten GebruikerRechten { get; set; } = default!;
    }

    public async Task<bool> IsUserLoggedInAsync()
    {
        var token = await SecureStorage.Default.GetAsync("jwt_token");
        Debug.WriteLine($"Token: {token}");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<bool> LogoutAsync()
    {
        try
        {
            bool exists = await SecureStorage.GetAsync("jwt_token") != null;
            if (exists)
            {
                SecureStorage.Remove("jwt_token");
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<Gebruiker>> SearchUsersAsync(string query)
    {
        var response = await _httpClient.GetAsync($"http://localhost:7153/api/account/searchUsers?query={query}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Gebruiker>>(content)!;
        }

        return new List<Gebruiker>();
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> DeleteUserAsync(string userId)
    {
        var response = await _httpClient.DeleteAsync($"http://localhost:7153/api/account/deleteUser?userId={userId}");

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent);
        }
    }

    public async Task<(bool IsSuccess, List<string> Roles)> GetUserRolesAsync(string userId)
    {
        await SetBearerToken();
        var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/account/getUserRoles?userId={userId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<string>>(content);
            return (true, roles);
        }
        return (false, new List<string>());
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> SetUserRolesAsync(string userId, List<string> roles)
    {
        var setUserRolesModel = new
        {
            UserId = userId,
            Roles = roles
        };

        var json = JsonConvert.SerializeObject(setUserRolesModel);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), "http://localhost:7153/api/account/setUserRoles")
        {
            Content = content
        };

        var response = await _httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            return (false, errorContent);
        }
    }

    public async Task<bool> UserHasRoleAsync(string userId, string roleName)
    {
        await SetBearerToken();
        var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/account/getUserRoles?userId={userId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var roles = JsonConvert.DeserializeObject<List<string>>(content);
            return roles!.Contains(roleName);
        }
        return false;
    }

    private async Task SetBearerToken()
    {
        var token = await SecureStorage.GetAsync("jwt_token");

        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }

    //Events
    public async Task<List<Evenement>> GetAllEventsAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Event");

            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Evenement>>(content);
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new List<Evenement>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetAllProductensAsync: {ex.Message}");
            return new List<Evenement>();
        }
    }

    //Producten zowel menu als stock
    public async Task<List<Product>> GetAllProductenAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Product");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Product>>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new List<Product>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetAllProductensAsync: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<Product> GetProductByIdAsync(int productId)
    {
        try
        {
            // Stuur een API-verzoek naar de backend om het product op te halen op basis van de ID
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Product/{productId}");
            var content = await response.Content.ReadAsStringAsync();
            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Product>(content);
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new Product();
            }
        }
        catch (Exception ex)
        {
            // Handel eventuele fouten af
            Console.WriteLine($"Fout bij het ophalen van productdetails: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteEventAsync(int eventId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de eventId
            var deleteUrl = $"{ipAdres}:7153/api/Event/{eventId}";

            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verwijderen succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in DeleteEventAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> DeleteProductAsync(int productId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de productId
            var deleteUrl = $"{ipAdres}:7153/api/Product/{productId}";

            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verwijderen succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in DeleteProductAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateEventAsync(Evenement newEvent)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Serializeer het nieuwe evenement naar JSON
            var jsonEvent = JsonConvert.SerializeObject(newEvent);

            // Bouw de URL voor het aanmaken van een nieuw evenement
            var createUrl = $"{ipAdres}:7153/api/Event";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateEventAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateProductAsync(Product newProduct)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            var jsonEvent = JsonConvert.SerializeObject(newProduct);

            // Bouw de URL voor het aanmaken van een nieuw product
            var createUrl = $"{ipAdres}:7153/api/Product";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateProductAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateEventAsync(int eventId, Evenement updatedEvent)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Converteer het bijgewerkte evenement naar JSON
            var jsonEvent = JsonConvert.SerializeObject(updatedEvent);

            // Maak een HTTP PUT-verzoek naar het juiste API-eindpunt om het evenement bij te werken
            var response = await _httpClient.PutAsync($"{ipAdres}:7153/api/Event/{eventId}",
                                                        new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in UpdateEventAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateProductAsync(int productId, Product updatedProduct)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Converteer het bijgewerkte product naar JSON
            var jsonEvent = JsonConvert.SerializeObject(updatedProduct);

            // Maak een HTTP PUT-verzoek naar de juiste API-eindpunt om het product bij te werken
            var response = await _httpClient.PutAsync($"{ipAdres}:7153/api/Product/{productId}",
                                                       new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in UpdateProductAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<HttpResponseMessage> RegisterUserForEventAsync(HttpContent content)
    {
        await SetBearerToken();  // Zorg ervoor dat de token is ingesteld voor authenticatie

        var response = await _httpClient.PatchAsync($"{ipAdres}:7153/api/event/registerToEvent", content);
        return response;
    }

    public async Task<HttpResponseMessage> UnregisterUserFromEventAsync(string userId, int eventId)
    {
        await SetBearerToken();  // Zorg ervoor dat de token is ingesteld voor authenticatie

        var url = $"{ipAdres}:7153/api/event/unregisterFromEvent/{userId}/{eventId}";

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
        var response = await _httpClient.SendAsync(request);

        return response;
    }

    public async Task<bool> IsUserRegisteredForEventAsync(int eventId)
    {
        await SetBearerToken();

        var userId = await SecureStorage.GetAsync("user_id");
        if (string.IsNullOrEmpty(userId))
        {
            Debug.WriteLine("User ID is not available.");
            return false;
        }

        var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/event/isUserRegistered?eventId={eventId}&userId={userId}");
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<bool>(content);
        }

        return false;
    }

    public async Task<List<Community>> GetAllCommunitiesAsync()
    {
        await SetBearerToken(); // Ensure the user is authenticated
        var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Community"); // Adjust the endpoint as necessary
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var communities = JsonConvert.DeserializeObject<List<Community>>(content);
            return communities ?? new List<Community>(); // Return the list of communities or an empty list if null
        }
        else
        {
            Debug.WriteLine($"Failed to load communities: {response.StatusCode}");
            return new List<Community>(); // Return an empty list on failure
        }
    }

    public async Task<List<Evenement>> GetEventsByCommunityAsync(int communityId)
    {
        await SetBearerToken();
        Debug.Print("GetEventsBycommunityAsync wordt afgevuurd");
        var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Event/byCommunity/{communityId}");

        Debug.WriteLine(response.Content);
        if (response.IsSuccessStatusCode)
        {
            Debug.WriteLine("Dit wordt ook afgevuurd");
            var content = await response.Content.ReadAsStringAsync();
            var events = JsonConvert.DeserializeObject<List<Evenement>>(content);
            return events ?? new List<Evenement>(); // Zorg ervoor dat er geen null geretourneerd wordt
        }
        return new List<Evenement>();
    }

    //Community
    public async Task<bool> CreateCommunityAsync(Community newCommunty)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Serializeer het nieuwe evenement naar JSON
            var jsonEvent = JsonConvert.SerializeObject(newCommunty);

            // Bouw de URL voor het aanmaken van een nieuw evenement
            var createUrl = $"{ipAdres}:7153/api/Community";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateCommunityAsync: {ex.Message}");
            return false;
        }
    }

    //Bestellingen

    public async Task<List<Bestelling>> GetAllBestellingenAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Bestelling");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Bestelling>>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new List<Bestelling>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetAllBestellingenAsync: {ex.Message}");
            return new List<Bestelling>();
        }
    }

    public async Task<bool> DeleteBestellingAsync(int bestellingId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de productId
            var deleteUrl = $"{ipAdres}:7153/api/Bestelling/{bestellingId}";

            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verwijderen succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in DeleteBestellingAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateBestellingAsync(Bestelling newBestelling)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar
            var jsonSettings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            };
            // Serializeer het nieuwe evenement naar JSON
            var jsonEvent = JsonConvert.SerializeObject(newBestelling, jsonSettings);
            Debug.WriteLine($"JSON payload: {jsonEvent}");
            // Bouw de URL voor het aanmaken van een nieuw product
            var createUrl = $"{ipAdres}:7153/api/Bestelling";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateBestellingAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateBestellingAsync(int bestellingId, Bestelling updatedBestelling)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Converteer het bijgewerkte product naar JSON
            var jsonEvent = JsonConvert.SerializeObject(updatedBestelling);

            // Maak een HTTP PUT-verzoek naar de juiste API-eindpunt om het product bij te werken
            var response = await _httpClient.PatchAsync($"{ipAdres}:7153/api/Bestelling/{bestellingId}",
                                                       new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in UpdateBestellingAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<Bestelling> GetBestellingByIdAsync(int bestellingId)
    {
        try
        {
            // Stuur een API-verzoek naar de backend om het product op te halen op basis van de ID
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Bestelling/{bestellingId}");
            var content = await response.Content.ReadAsStringAsync();
            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Bestelling>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new Bestelling();
            }
        }
        catch (Exception ex)
        {
            // Handel eventuele fouten af
            Console.WriteLine($"Fout bij het ophalen van productdetails: {ex.Message}");
            throw;
        }
    }

    // Gebruikers
    public async Task<List<Gebruiker>> GetAllGebruikersAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Gebruiker");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Gebruiker>>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new List<Gebruiker>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetAllGebruikersAsync: {ex.Message}");
            return new List<Gebruiker>();
        }
    }

    public async Task<Gebruiker> GetGebruikerByIdAsync(string gebruikerId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie voor het ophalen van de gebruiker op basis van ID
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Gebruiker/{gebruikerId}");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Gebruiker>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return null!;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetGebruikerByIdAsync: {ex.Message}");
            return null!;
        }
    }

    public async Task<Gebruiker> GetCurrentGebruikerAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie voor het ophalen van de huidige gebruiker-ID
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Gebruiker/current");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Gebruiker>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return null!;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetCurrentUserIdAsync: {ex.Message}");
            return null!;
        }
    }

    public async Task<bool> DeleteGebruikerAsync(string gebruikerId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de productId
            var deleteUrl = $"{ipAdres}:7153/api/Gebruiker/{gebruikerId}";

            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verwijderen succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in DeleteGebruikerAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateGebruikerAsync(Gebruiker newGebruiker)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            var jsonEvent = JsonConvert.SerializeObject(newGebruiker);

            // Bouw de URL voor het aanmaken van een nieuw product
            var createUrl = $"{ipAdres}:7153/api/Gebruiker";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> DeleteCommunityAsync(int communityId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de eventId
            var deleteUrl = $"{ipAdres}:7153/api/Community/{communityId}";
            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> UpdateGebruikerAsync(string gebruikerId, Gebruiker updatedGebruiker)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Converteer het bijgewerkte product naar JSON
            var jsonEvent = JsonConvert.SerializeObject(updatedGebruiker);

            // Maak een HTTP PUT-verzoek naar de juiste API-eindpunt om het product bij te werken
            var response = await _httpClient.PutAsync($"{ipAdres}:7153/api/Gebruiker/{gebruikerId}",
                                                       new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in UpdateGebruikerAsync: {ex.Message}");
            return false;
        }
    }

    // Bestelde Producten

    public async Task<List<BesteldProduct>> GetAllBesteldeProductenAsync()
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
            var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/BesteldProduct");
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"API Response: {content}");
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<BesteldProduct>>(content)!;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return new List<BesteldProduct>();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in GetAllBesteldeProductensAsync: {ex.Message}");
            return new List<BesteldProduct>();
        }
    }

    public async Task<bool> DeleteBesteldProductAsync(int BesteldProductId)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Bouw de URL op met de productId
            var deleteUrl = $"{ipAdres}:7153/api/BesteldProduct/{BesteldProductId}";

            // Verzend een DELETE-verzoek naar de API
            var response = await _httpClient.DeleteAsync(deleteUrl);

            // Controleer of het verwijderen succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                Debug.WriteLine($"API Error: {response.StatusCode}");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in DeleteBesteldProductAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> CreateBesteldProductAsync(BesteldProduct newBesteldProduct)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Serializeer het nieuwe evenement naar JSON
            var jsonEvent = JsonConvert.SerializeObject(newBesteldProduct);

            // Bouw de URL voor het aanmaken van een nieuw product
            var createUrl = $"{ipAdres}:7153/api/BesteldProduct";

            // Maak een nieuw HTTP POST-verzoek met het JSON-evenement als inhoud
            var response = await _httpClient.PostAsync(createUrl, new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het aanmaken succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log de volledige foutmelding van de API-respons
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in CreateBesteldProductAsync: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateBesteldProductAsync(int BesteldProductId, BesteldProduct updatedBesteldProduct)
    {
        try
        {
            await SetBearerToken(); // Stel de token in indien beschikbaar

            // Converteer het bijgewerkte product naar JSON
            var jsonEvent = JsonConvert.SerializeObject(updatedBesteldProduct);

            // Maak een HTTP PUT-verzoek naar de juiste API-eindpunt om het product bij te werken
            var response = await _httpClient.PutAsync($"{ipAdres}:7153/api/BesteldProduct/{BesteldProductId}",
                                                       new StringContent(jsonEvent, Encoding.UTF8, "application/json"));

            // Controleer of het verzoek succesvol was
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                // Log error of handel foutieve status codes af
                string errorMessage = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Error: {response.StatusCode}, {errorMessage}");

                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in UpdateBesteldProductAsync: {ex.Message}");
            return false;
        }
    }
}