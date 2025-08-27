export class CurrencyUtil {
  /**
   * Formats amount in Sri Lankan Rupees format: Rs. 98,000/=
   * @param amount - The amount to format
   * @returns Formatted currency string
   */
  static formatAmount(amount: number): string {
    const formattedNumber = new Intl.NumberFormat('en-US').format(Math.abs(amount));
    const sign = amount < 0 ? '-' : '';
    return `${sign}Rs. ${formattedNumber}/=`;
  }

  /**
   * Gets the CSS class for balance styling based on amount
   * @param amount - The balance amount
   * @returns CSS class name
   */
  static getBalanceClass(amount: number): string {
    return amount >= 0 ? 'positive-balance' : 'negative-balance';
  }
}
