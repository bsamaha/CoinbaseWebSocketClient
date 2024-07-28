using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoinbaseWebSocketClient
{
    public class RateLimiter
    {
        private readonly double _maxBurst;
        private readonly double _refillRate;
        private double _tokens;
        private DateTime _lastRefillTime;

        public RateLimiter(double maxBurst, double refillRate)
        {
            _maxBurst = maxBurst;
            _refillRate = refillRate;
            _tokens = maxBurst;
            _lastRefillTime = DateTime.UtcNow;
        }

        public async Task WaitForTokenAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                RefillTokens();
                if (_tokens >= 1)
                {
                    _tokens -= 1;
                    return;
                }
                await Task.Delay(TimeSpan.FromMilliseconds(10), cancellationToken);
            }
        }

        private void RefillTokens()
        {
            var now = DateTime.UtcNow;
            var elapsedSeconds = (now - _lastRefillTime).TotalSeconds;
            var newTokens = elapsedSeconds * _refillRate;
            _tokens = Math.Min(_maxBurst, _tokens + newTokens);
            _lastRefillTime = now;
        }
    }
}