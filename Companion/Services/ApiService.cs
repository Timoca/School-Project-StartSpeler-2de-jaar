using Newtonsoft.Json;
using System.Text;
using Companion.Models;
using System.Net.Http;
using System.Diagnostics;

namespace Companion.Services
{
    public class ApiService
    {
        //Login/Logout + Registratie
        private readonly HttpClient _httpClient;

        //Niet vergeten dit aan te passen naargelang het ipadres van computer
        private string ipAdres = "http://192.168.1.6";

        public ApiService()
        {
            _httpClient = new HttpClient(new HttpClientHandler());
        }

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

            try
            {
                var response = await _httpClient.PostAsync($"{ipAdres}:7153/api/account/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    await SecureStorage.SetAsync("jwt_token", token); // Opslaan van de token
                    await SetBearerTokenAsync(); // Stel de token in direct na het inloggen
                    Gebruiker gebruiker = await GetUserDetailsAsync(email); // Haal gebruiker gegevens op
                    return token;
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during HttpClient call: {ex.Message}");
            }

            return null!;
        }

        public async Task<bool> IsUserLoggedInAsync()
        {
            var token = await SecureStorage.GetAsync("jwt_token");
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

        //Verkrijg userid
        public async Task<Gebruiker> GetUserDetailsAsync(string email)
        {
            await SetBearerTokenAsync();

            try
            {
                var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/account/detailsByEmail?email={Uri.EscapeDataString(email)}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var user = JsonConvert.DeserializeObject<Gebruiker>(content);
                    await SecureStorage.SetAsync("user_id", user!.Id.ToString()); // Opslaan van UserId
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

        //Bearer token
        private async Task SetBearerTokenAsync()
        {
            try
            {
                var token = await SecureStorage.GetAsync("jwt_token");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                    Debug.WriteLine($"Bearer token set: {_httpClient.DefaultRequestHeaders.Authorization}");
                }
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                Console.WriteLine("Failed to set bearer token: " + ex.Message);
            }
        }

        //Events
        public async Task<List<Evenement>> GetAllEventsAsync()
        {
            try
            {
                await SetBearerTokenAsync(); // Stel de token in indien beschikbaar

                // Directe volledige URL specificatie, vergelijkbaar met de login- en registermethoden
                var response = await _httpClient.GetAsync($"{ipAdres}:7153/api/Event");

                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"API Response: {content}");
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<Evenement>>(content)!;
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
                Debug.WriteLine($"Exception in GetAllEventsAsync: {ex.Message}");
                return new List<Evenement>();
            }
        }

        public async Task<HttpResponseMessage> RegisterUserForEventAsync(HttpContent content)
        {
            await SetBearerTokenAsync();  // Zorg ervoor dat de token is ingesteld voor authenticatie

            var response = await _httpClient.PatchAsync($"{ipAdres}:7153/api/event/registerToEvent", content);
            return response;
        }

        public async Task<HttpResponseMessage> UnregisterUserFromEventAsync(string userId, int eventId)
        {
            await SetBearerTokenAsync();  // Zorg ervoor dat de token is ingesteld voor authenticatie

            var url = $"{ipAdres}:7153/api/event/unregisterFromEvent/{userId}/{eventId}";

            var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
            var response = await _httpClient.SendAsync(request);

            return response;
        }

        public async Task<bool> IsUserRegisteredForEventAsync(int eventId)
        {
            await SetBearerTokenAsync();

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
            await SetBearerTokenAsync(); // Ensure the user is authenticated
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
            await SetBearerTokenAsync();
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

        //Dranken
        public async Task<List<Product>> GetAllProductsAsync()
        {
            try
            {
                await SetBearerTokenAsync(); // Stel de token in indien beschikbaar

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

        public async Task<HttpResponseMessage> PlaatsBestellingAsync(Bestelling bestelling)
        {
            try
            {
                await SetBearerTokenAsync();

                var json = JsonConvert.SerializeObject(bestelling);
                Debug.WriteLine($"JSON payload: {json}");
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{ipAdres}:7153/api/Bestelling", content);
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Fout bij het plaatsen van de bestelling: {ex.Message}");
                throw; // Opnieuw gooien om de fout door te geven naar de aanroepende code
            }
        }
    }
}