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
    public async Task<IActionResult> GetAll()
    {
        var movies = await movieRepository.GetAllAsync();

        var moviesResponse = movies.MapToMoviesResponse();

        return Ok(moviesResponse);
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get(string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) ? 
            await movieRepository.GetByIdAsync(id) :
            await movieRepository.GetBySlugAsync(idOrSlug);

        if (movie is null)
        {
            return NotFound();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return Ok(movieResponse);
    }

    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create(CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        
        var createdSuccessfully = await movieRepository.CreateAsync(movie);

        if (!createdSuccessfully)
        {
            return BadRequest();
        }
        
        var movieResponse = movie.MapToMovieResponse();

        return CreatedAtAction(nameof(Get), new { idOrSlug = movie.Id }, movieResponse);
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update(Guid id, UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);

        var updatedMovie = await movieRepository.UpdateAsync(movie);

        if (updatedMovie is null)
        {
            return NotFound();
        }

        return Ok(updatedMovie);
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deletedSuccessfully = await movieRepository.DeleteByIdAsync(id);

        if (!deletedSuccessfully)
        {
            return NotFound();
        }

        return Ok();
    }
}