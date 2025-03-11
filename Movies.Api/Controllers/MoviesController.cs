using Microsoft.AspNetCore.Mvc;
using Movies.Api.Mapping;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieService movieRepository) : ControllerBase
{
    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll(CancellationToken token)
    {
        var movies = await movieRepository.GetAllAsync(token);

        var moviesResponse = movies.MapToMoviesResponse();

        return Ok(moviesResponse);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get(string idOrSlug, CancellationToken token)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) ? 
            await movieRepository.GetByIdAsync(id, token) :
            await movieRepository.GetBySlugAsync(idOrSlug, token);

        if (movie is null)
        {
            return NotFound();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return Ok(movieResponse);
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(CreateMovieRequest request, CancellationToken token)
    {
        var movie = request.MapToMovie();
        
        var createdSuccessfully = await movieRepository.CreateAsync(movie, token);

        if (!createdSuccessfully)
        {
            return BadRequest();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movieResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateMovieRequest request, CancellationToken token)
    {
        var movie = request.MapToMovie(id);

        var updatedMovie = await movieRepository.UpdateAsync(movie, token);

        if (updatedMovie is null)
        {
            return NotFound();
        }

        return Ok(updatedMovie);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken token)
    {
        var deletedSuccessfully = await movieRepository.DeleteByIdAsync(id, token);

        if (!deletedSuccessfully)
        {
            return NotFound();
        }

        return Ok();
    }
}