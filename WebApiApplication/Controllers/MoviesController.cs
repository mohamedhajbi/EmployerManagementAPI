using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Models;
using MoviesApi.Services;
using WebApiApplication.Dtos;
using WebApiApplication.Services;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;

        private readonly List<string> _allowedExtensions = new List<string> { ".jpg", ".png" };
        private const long _maxAllowedPosterSize = 1048576; // 1 MB

        public MoviesController(IMoviesService moviesService, IGenresService genresService, IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }

        // GET: api/movies
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesService.GetAll();
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(data);
        }

        // GET: api/movies/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null)
                return NotFound();

            var dto = _mapper.Map<MovieDetailsDto>(movie);
            return Ok(dto);
        }

        // DELETE: api/movies/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null)
                return NotFound($"No movie was found with ID {id}");

            _moviesService.Delete(movie);
            return Ok(movie);
        }

        // GET: api/movies/GetByGenreId/{genreId}
        [HttpGet("GetByGenreId/{genreId}")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies = await _moviesService.GetAll(genreId);
            var data = _mapper.Map<IEnumerable<MovieDetailsDto>>(movies);
            return Ok(data);
        }

        // POST: api/movies
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromForm] MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is required.");

            if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and .jpg images are allowed.");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest("Max allowed size for poster is 1MB!");

            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId.Value);
            if (!isValidGenre)
                return BadRequest("Invalid genre ID");

            using var dataStream = new MemoryStream();
            await dto.Poster.CopyToAsync(dataStream);

            var movie = _mapper.Map<Movie>(dto);
            movie.Poster = dataStream.ToArray();

            await _moviesService.Add(movie);
            return Ok(movie);
        }

        // PUT: api/movies/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);
            if (movie == null)
                return NotFound($"No movie was found with ID {id}");

            var isValidGenre = await _genresService.IsvalidGenre(dto.GenreId.Value);
            if (!isValidGenre)
                return BadRequest("Invalid genre ID");

            if (dto.Poster != null)
            {
                if (!_allowedExtensions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and .jpg images are allowed.");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest("Max allowed size for poster is 1MB!");

                using var dataStream = new MemoryStream();
                await dto.Poster.CopyToAsync(dataStream);
                movie.Poster = dataStream.ToArray();
            }

            movie.Title = dto.Title;
            movie.GenreId = dto.GenreId;
            movie.Year = dto.Year;
            movie.Storeline = dto.Storyline;
            movie.Rate = dto.Rate;

            _moviesService.Update(movie);
            return Ok(movie);
        }
    }
}
